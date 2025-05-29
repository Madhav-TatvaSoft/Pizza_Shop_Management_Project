
CREATE OR REPLACE FUNCTION get_order_details_by_customer_id( inp_customer_id BIGINT )
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
    order_details_json JSON;
    d_order_id BIGINT;
BEGIN
	
    -- Get the order_id from AssignTables
    
	SELECT at.order_id
    INTO d_order_id
    FROM "assignTable" at
    WHERE at.customer_id = inp_customer_id
    AND at.isdelete = false
    LIMIT 1;

    -- If no valid order_id is found, set to 0
    
	IF d_order_id IS NULL THEN
        d_order_id := 0;
    END IF;

    -- Main query to build the JSON response
    
	SELECT json_build_object(
        'OrderId', d_order_id,
        'PaymentmethodId', 4,
        -- Table Details
        'SectionId', t.section_id,
        'SectionName', s.section_name,
        'tableList', (
            SELECT COALESCE(json_agg(json_build_object(
                'TableId', at2.table_id,
                'TableName', t2.table_name,
                'Capacity', t2.capacity,
                'SectionId', t2.section_id
            )), '[]'::JSON)
            FROM "assignTable" at2
            JOIN tables t2 ON at2.table_id = t2.table_id
            WHERE at2.customer_id = inp_customer_id
            AND at2.isdelete = false
        ),
        -- Customer Details
        'CustomerId', c.customer_id,
        'CustomerName', c.customer_name,
        'PhoneNo', c.phone_no,
        'Email', c.email,
        'NoOfPerson', (
            SELECT at3.no_of_person
            FROM "assignTable" at3
            WHERE at3.customer_id = inp_customer_id
            AND at3.isdelete = false
            LIMIT 1
        ),
        -- Order Details (if order_id exists)
        'Status', CASE WHEN d_order_id != 0 THEN (
            SELECT o.status
            FROM orders o
            WHERE o.order_id = d_order_id
            AND o.isdelete = false
        ) ELSE NULL END,
        'OrderInstruction', CASE WHEN d_order_id != 0 THEN (
            SELECT o.extra_instruction
            FROM orders o
            WHERE o.order_id = d_order_id
            AND o.isdelete = false
        ) ELSE NULL END,
        'InvoiceId', CASE WHEN d_order_id != 0 THEN (
            SELECT COALESCE((
                SELECT i.invoice_id
                FROM invoice i
                WHERE i.order_id = d_order_id
                AND i.customer_id = inp_customer_id
                AND i.isdelete = false
                LIMIT 1
            ), 0)
        ) ELSE 0 END,
        'OrderDate', CASE WHEN d_order_id != 0 THEN (
            SELECT o2.order_date
            FROM "assignTable" at4
            JOIN orders o2 ON at4.order_id = o2.order_id
            WHERE at4.customer_id = inp_customer_id
            AND at4.isdelete = false
            LIMIT 1
        ) ELSE NULL END,
        'ModifiedOn', CASE WHEN d_order_id != 0 THEN (
            SELECT COALESCE((
                SELECT o2.modified_at
                FROM "assignTable" at5
                JOIN orders o2 ON at5.order_id = o2.order_id
                WHERE at5.customer_id = inp_customer_id
                AND at5.isdelete = false
                LIMIT 1
            ), (
                SELECT o2.order_date
                FROM "assignTable" at5
                JOIN orders o2 ON at5.order_id = o2.order_id
                WHERE at5.customer_id = inp_customer_id
                AND at5.isdelete = false
                LIMIT 1
            ))
        ) ELSE NULL END,
        'ratingVM', json_build_object(),
        'itemOrderVM', CASE WHEN d_order_id != 0 THEN (
            SELECT COALESCE(json_agg(json_build_object(
                'ItemId', od.item_id,
                'ItemName', i.item_name,
                'Rate', i.rate,
                'status', 'In Progress',
                'Quantity', od.quantity,
                'ExtraInstruction', COALESCE(od.extra_instruction, ''),
                'OrderdetailId', od.orderdetail_id,
                'TotalItemAmount', ROUND(od.quantity * i.rate, 2),
                'modifierOrderVM', (
                    SELECT COALESCE(json_agg(json_build_object(
                        'ModifierId', m.modifier_id,
                        'ModifierName', m.modifier_name,
                        'Rate', m.rate,
                        'Quantity', od.quantity,
                        'TotalModifierAmount', ROUND(od.quantity * m.rate, 2)
                    ) ORDER BY m.modifier_id), '[]'::JSON)
                    FROM modifierorder mo
                    JOIN modifier m ON mo.modifier_id = m.modifier_id
                    WHERE mo.orderdetail_id = od.orderdetail_id
                    AND mo.isdelete = false
                )
            )), '[]'::JSON)
            FROM orderdetails od
            JOIN items i ON od.item_id = i.item_id
            WHERE od.order_id = d_order_id
            AND od.isdelete = false
        ) ELSE '[]'::JSON END,
        'SubTotalAmountOrder', CASE WHEN d_order_id != 0 THEN (
            SELECT ROUND(SUM(
                (od.quantity * i.rate) + (
                    SELECT COALESCE(SUM(od.quantity * m.rate), 0)
                    FROM modifierorder mo
                    JOIN modifier m ON mo.modifier_id = m.modifier_id
                    WHERE mo.orderdetail_id = od.orderdetail_id
                    AND mo.isdelete = false
                )
            ), 2)
            FROM orderdetails od
            JOIN items i ON od.item_id = i.item_id
            WHERE od.order_id = d_order_id
            AND od.isdelete = false
        ) ELSE 0 END,
        'taxInvoiceVM', (
            SELECT COALESCE(json_agg(json_build_object(
                'TaxId', t.tax_id,
                'TaxName', t.tax_name,
                'TaxType', t.tax_type,
                'TaxValue', CASE
                    WHEN t.tax_type = 'Flat Amount' THEN t.tax_value
                    ELSE ROUND(t.tax_value / 100.0 * (
                        SELECT COALESCE(SUM(
                            (od.quantity * i.rate) + (
                                SELECT COALESCE(SUM(od.quantity * m.rate), 0)
                                FROM modifierorder mo
                                JOIN modifier m ON mo.modifier_id = m.modifier_id
                                WHERE mo.orderdetail_id = od.orderdetail_id
                                AND mo.isdelete = false
                            )
                        ), 0)
                        FROM orderdetails od
                        JOIN items i ON od.item_id = i.item_id
                        WHERE od.order_id = d_order_id
                        AND od.isdelete = false
                    ), 2)
                END
            )), '[]'::JSON)
            FROM tax t
            WHERE t.isdelete = false
            AND t.isenable = true
        ),
        'TotalAmountOrder', (
            SELECT ROUND(
                COALESCE((
                    SELECT SUM(
                        (od.quantity * i.rate) + (
                            SELECT COALESCE(SUM(od.quantity * m.rate), 0)
                            FROM modifierorder mo
                            JOIN modifier m ON mo.modifier_id = m.modifier_id
                            WHERE mo.orderdetail_id = od.orderdetail_id
                            AND mo.isdelete = false
                        )
                    )
                    FROM orderdetails od
                    JOIN items i ON od.item_id = i.item_id
                    WHERE od.order_id = d_order_id
                    AND od.isdelete = false
                ), 0) + COALESCE((
                    SELECT SUM(
                        CASE
                            WHEN t.tax_type = 'Flat Amount' THEN t.tax_value
                            ELSE ROUND(t.tax_value / 100.0 * (
                                SELECT COALESCE(SUM(
                                    (od.quantity * i.rate) + (
                                        SELECT COALESCE(SUM(od.quantity * m.rate), 0)
                                        FROM modifierorder mo
                                        JOIN modifier m ON mo.modifier_id = m.modifier_id
                                        WHERE mo.orderdetail_id = od.orderdetail_id
                                        AND mo.isdelete = false
                                    )
                                ), 0)
                                FROM orderdetails od
                                JOIN items i ON od.item_id = i.item_id
                                WHERE od.order_id = d_order_id
                                AND od.isdelete = false
                            ), 2)
                        END
                    )
                    FROM tax t
                    WHERE t.isdelete = false
                    AND t.isenable = true
                ), 0), 2)
        )
    )
    INTO order_details_json
    FROM customers c
    JOIN "assignTable" at ON c.customer_id = at.customer_id
    JOIN tables t ON at.table_id = t.table_id
    JOIN sections s ON t.section_id = s.section_id
    WHERE c.customer_id = inp_customer_id
    AND c.isdelete = false
    AND at.isdelete = false
    LIMIT 1;

    RETURN order_details_json;

END;
$$;