CREATE OR REPLACE PROCEDURE save_order_details_orderapp(
    IN inp_orderDetailIds JSONB,
    IN inp_orderDetailsVM JSONB,
    IN inp_userId BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_orderId BIGINT;
    v_invoiceId BIGINT;
    v_item JSONB;
    v_modifier JSONB;
    v_table JSONB;
    v_tax JSONB;
    v_orderDetailId BIGINT;
    v_subTotalAmountOrder NUMERIC;
    v_TotalAmountOrder NUMERIC;
    v_customerId BIGINT;
    v_orderInstruction TEXT;
    v_paymentMethodId INTEGER;
    v_sectionId INTEGER;
    v_tableId INTEGER;
    v_index INTEGER;
    v_orderId_text TEXT;
    v_invoiceId_text TEXT;
    v_temp_value TEXT; -- For validating inp_orderDetailIds
BEGIN
    -- Start transaction
    BEGIN
        -- Validate inp_orderDetailIds
        IF inp_orderDetailIds IS NOT NULL AND jsonb_typeof(inp_orderDetailIds) != 'array' THEN
            RAISE EXCEPTION 'inp_orderDetailIds must be a JSONB array';
        END IF;

        -- Validate inp_orderDetailsVM
        IF inp_orderDetailsVM IS NULL OR jsonb_typeof(inp_orderDetailsVM) != 'object' THEN
            RAISE EXCEPTION 'inp_orderDetailsVM must be a JSONB object';
        END IF;

        -- Extract common fields with null checks
        v_subTotalAmountOrder := COALESCE((inp_orderDetailsVM->>'SubTotalAmountOrder')::NUMERIC, 0);
        v_TotalAmountOrder := COALESCE((inp_orderDetailsVM->>'TotalAmountOrder')::NUMERIC, 0);
        v_customerId := COALESCE((inp_orderDetailsVM->>'CustomerId')::BIGINT, NULL);
        v_orderInstruction := inp_orderDetailsVM->>'OrderInstruction';
        v_paymentMethodId := COALESCE((inp_orderDetailsVM->>'PaymentmethodId')::INTEGER, 4);
        v_sectionId := COALESCE((inp_orderDetailsVM->>'SectionId')::INTEGER, NULL);
        v_tableId := COALESCE((inp_orderDetailsVM->'tableList'->0->>'TableId')::INTEGER, NULL);

        -- Debug: Log extracted values
        RAISE NOTICE 'Extracted values: SubTotalAmountOrder=%, TotalAmountOrder=%, CustomerId=%, PaymentMethodId=%, SectionId=%, TableId=%',
            v_subTotalAmountOrder, v_TotalAmountOrder, v_customerId, v_paymentMethodId, v_sectionId, v_tableId;

        -- Extract and validate OrderId
        v_orderId_text := inp_orderDetailsVM->>'OrderId';
        IF v_orderId_text IS NULL OR v_orderId_text = '' OR v_orderId_text = '0' THEN
            -- Create new order
            RAISE NOTICE 'Creating new order';
            INSERT INTO orders (
                customer_id, order_date, status, total_amount, paymentmethod_id,
                payment_status_id, section_id, table_id, extra_instruction,
                created_by, created_at, "orderType"
            )
            VALUES (
                v_customerId, NOW(), 'Pending', v_TotalAmountOrder,
                v_paymentMethodId, 1, v_sectionId, v_tableId,
                v_orderInstruction, inp_userId, NOW(), 'DineIn'
            )
            RETURNING order_id INTO v_orderId;
            RAISE NOTICE 'New order created with order_id: %', v_orderId;
        ELSE
            -- Validate OrderId is numeric
            IF v_orderId_text ~ '^[0-9]+$' THEN
                v_orderId := v_orderId_text::BIGINT;
                RAISE NOTICE 'Using existing order_id: %', v_orderId;
                -- Update existing order
                UPDATE orders
                SET total_amount = v_TotalAmountOrder,
                    paymentmethod_id = v_paymentMethodId,
                    payment_status_id = 1,
                    extra_instruction = v_orderInstruction,
                    modified_by = inp_userId,
                    modified_at = NOW()
                WHERE order_id = v_orderId AND isdelete = FALSE;
            ELSE
                RAISE EXCEPTION 'Invalid OrderId format: %', v_orderId_text;
            END IF;
        END IF;

        -- Update inp_orderDetailsVM with orderId
        inp_orderDetailsVM := jsonb_set(inp_orderDetailsVM, '{OrderId}', to_jsonb(v_orderId));

        -- Add new order details
        FOR v_item IN 
            SELECT value 
            FROM jsonb_array_elements(inp_orderDetailsVM->'itemOrderVM') 
            WITH ORDINALITY 
            WHERE ordinality > COALESCE(jsonb_array_length(inp_orderDetailIds), 0)
        LOOP
            -- Validate ItemId
            IF (v_item->>'ItemId') IS NULL OR NOT ((v_item->>'ItemId') ~ '^[0-9]+$') THEN
                RAISE EXCEPTION 'Invalid ItemId in itemOrderVM: %', v_item->>'ItemId';
            END IF;

            -- Insert order detail
            INSERT INTO orderdetails (
                order_id, item_id, quantity, extra_instruction, status,
                created_at, created_by
            )
            VALUES (
                v_orderId,
                (v_item->>'ItemId')::BIGINT,
                COALESCE((v_item->>'Quantity')::INTEGER, 1),
                v_item->>'ExtraInstruction',
                'Pending',
                NOW(),
                inp_userId
            )
            RETURNING orderdetail_id INTO v_orderDetailId;

            RAISE NOTICE 'Inserted order detail with orderdetail_id: %', v_orderDetailId;

            -- Update itemOrderVM with orderDetailId
            inp_orderDetailsVM := jsonb_set(
                inp_orderDetailsVM,
                ARRAY['itemOrderVM', (jsonb_array_length(inp_orderDetailsVM->'itemOrderVM') - 1)::TEXT, 'OrderdetailId'],
                to_jsonb(v_orderDetailId)
            );

            -- Insert modifiers
            FOR v_modifier IN SELECT jsonb_array_elements(v_item->'modifierOrderVM')
            LOOP
                IF (v_modifier->>'ModifierId') IS NULL OR NOT ((v_modifier->>'ModifierId') ~ '^[0-9]+$') THEN
                    RAISE EXCEPTION 'Invalid ModifierId in modifierOrderVM: %', v_modifier->>'ModifierId';
                END IF;

                INSERT INTO modifierorder (
                    orderdetail_id, modifier_id, created_at, created_by, modifierQuantity
                )
                VALUES (
                    v_orderDetailId,
                    (v_modifier->>'ModifierId')::BIGINT,
                    NOW(),
                    inp_userId,
                    COALESCE((v_item->>'Quantity')::INTEGER, 1)
                );
            END LOOP;
        END LOOP;

        -- Update existing order details
        FOR v_item, v_index IN 
            SELECT value, ordinality - 1 AS idx 
            FROM jsonb_array_elements(inp_orderDetailIds) WITH ORDINALITY
        LOOP
            -- Validate orderDetailId is numeric
            v_temp_value := v_item #>> '{}';
            IF v_temp_value IS NULL OR NOT (v_temp_value ~ '^[0-9]+$') THEN
                RAISE EXCEPTION 'Invalid orderDetailId in inp_orderDetailIds: %', v_temp_value;
            END IF;

            UPDATE orderdetails
            SET quantity = COALESCE((inp_orderDetailsVM->'itemOrderVM'->v_index->>'Quantity')::INTEGER, 1),
                extra_instruction = inp_orderDetailsVM->'itemOrderVM'->v_index->>'ExtraInstruction',
                modified_at = NOW(),
                modified_by = inp_userId
            WHERE orderdetail_id = v_temp_value::BIGINT AND isdelete = FALSE;

            -- Update modifier quantities
            UPDATE modifierorder
            SET modifierQuantity = COALESCE((inp_orderDetailsVM->'itemOrderVM'->v_index->>'Quantity')::INTEGER, 1),
                modified_at = NOW(),
                modified_by = inp_userId
            WHERE orderdetail_id = v_temp_value::BIGINT;
        END LOOP;

        -- Update AssignTable and Table status
        FOR v_table IN SELECT jsonb_array_elements(inp_orderDetailsVM->'tableList')
        LOOP
            IF (v_table->>'TableId') IS NULL OR NOT ((v_table->>'TableId') ~ '^[0-9]+$') THEN
                RAISE EXCEPTION 'Invalid TableId in tableList: %', v_table->>'TableId';
            END IF;

            UPDATE "assignTable"
            SET order_id = v_orderId,
                modified_at = NOW(),
                modified_by = inp_userId
            WHERE table_id = (v_table->>'TableId')::INTEGER AND isdelete = FALSE;

            UPDATE "tables"
            SET Status = 'Running',
                modified_at = NOW(),
                modified_by = inp_userId
            WHERE table_id = (v_table->>'TableId')::INTEGER AND isdelete = FALSE;
        END LOOP;

        -- Update order status
        UPDATE orders
        SET status = 'In Progress'
        WHERE order_id = v_orderId AND isdelete = FALSE;

        -- Update itemOrderVM status
        inp_orderDetailsVM := jsonb_set(
            inp_orderDetailsVM,
            '{itemOrderVM}',
            (
                SELECT jsonb_agg(jsonb_set(item, '{status}', '"In Progress"'::jsonb))
                FROM jsonb_array_elements(inp_orderDetailsVM->'itemOrderVM') AS item
            )
        );

        -- Handle taxes
        inp_orderDetailsVM := jsonb_set(inp_orderDetailsVM, '{taxInvoiceVM}', '[]'::jsonb);
        FOR v_tax IN 
            SELECT tax_id, tax_name, tax_type, tax_value
            FROM tax
            WHERE isdelete = FALSE AND isenable = TRUE
        LOOP
            IF v_tax.tax_type = 'Flat Amount' THEN
                inp_orderDetailsVM := jsonb_set(
                    inp_orderDetailsVM,
                    '{taxInvoiceVM}',
                    (inp_orderDetailsVM->'taxInvoiceVM') || jsonb_build_object(
                        'TaxId', v_tax.tax_id,
                        'TaxName', v_tax.tax_name,
                        'TaxType', v_tax.tax_type,
                        'TaxValue', v_tax.tax_value
                    )
                );
            ELSE
                inp_orderDetailsVM := jsonb_set(
                    inp_orderDetailsVM,
                    '{taxInvoiceVM}',
                    (inp_orderDetailsVM->'taxInvoiceVM') || jsonb_build_object(
                        'TaxId', v_tax.tax_id,
                        'TaxName', v_tax.tax_name,
                        'TaxType', v_tax.tax_type,
                        'TaxValue', ROUND(v_tax.tax_value / 100 * v_subTotalAmountOrder, 2)
                    )
                );
            END IF;
        END LOOP;

        -- Handle invoice
        v_invoiceId_text := inp_orderDetailsVM->>'InvoiceId';
        IF v_invoiceId_text IS NULL OR v_invoiceId_text = '' OR v_invoiceId_text = '0' THEN
            RAISE NOTICE 'Creating new invoice';
            INSERT INTO invoice (
                invoice_no, order_id, customer_id, created_at, created_by
            )
            VALUES (
                '#DOM' || TO_CHAR(NOW(), 'YYYYMMDDHH24MISS'),
                v_orderId,
                v_customerId,
                NOW(),
                inp_userId
            )
            RETURNING invoice_id INTO v_invoiceId;
            RAISE NOTICE 'New invoice created with invoice_id: %', v_invoiceId;

            inp_orderDetailsVM := jsonb_set(inp_orderDetailsVM, '{InvoiceId}', to_jsonb(v_invoiceId));
        END IF;

        -- Output the result
        RAISE NOTICE 'Final inp_orderDetailsVM: %', inp_orderDetailsVM::TEXT;

    EXCEPTION WHEN OTHERS THEN
        -- Rollback transaction on error
        RAISE EXCEPTION 'Error in save_order_details_orderapp: %', SQLERRM;
    END;
END;
$$;