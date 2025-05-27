----------------------------------------------------------------  OrderApp Menu Module --------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------

------------------------- GET ITEM LIST ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_item_list_orderapp(
    inp_categoryid BIGINT,
    inp_searchText TEXT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
    ItemList JSON;
BEGIN
    SELECT COALESCE(json_agg(row_to_json(list)), '[]'::JSON)
    INTO ItemList
    FROM (
        SELECT 
            i.item_id AS "ItemId",
            i.item_name AS "ItemName",
            i.category_id AS "CategoryId",
            i.item_type_id AS "ItemTypeId",
            CEIL(i.rate) AS "Rate",
            i.item_image AS "ItemImage",
            i."isFavourite" AS "IsFavourite",
            i.isdelete AS "Isdelete"
        FROM items i
        WHERE i.isavailable = true AND i.isdelete = false
        AND (
            (inp_categoryid = -1 AND i."isFavourite" = true)
            OR (inp_categoryid = 0)
            OR (inp_categoryid > 0 AND i.category_id = inp_categoryid)
        )
        AND (
            inp_searchText IS NULL OR TRIM(inp_searchText) = '' OR LOWER(i.item_name) LIKE '%' || LOWER(TRIM(inp_searchText)) || '%'
        )ORDER BY i.item_name
    ) list;

    IF ItemList IS NULL THEN
        RETURN NULL;
    END IF;

    RETURN ItemList;

END;
$$;

-- END --

------------------------- FAVOURITE ITEM ORDERAPP -----------------------------

CREATE OR REPLACE PROCEDURE favourite_item_orderapp(
	inp_itemid BIGINT,
	inp_isFavourite BOOLEAN,
	inp_userid BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_item_exists BOOLEAN;
BEGIN
    -- Begin transaction
    BEGIN
        -- Check if item exists
        SELECT EXISTS (
            SELECT 1 
            FROM items
            WHERE item_id = inp_itemid 
            AND isdelete = false 
        ) INTO v_item_exists;

        IF NOT v_item_exists THEN
            RAISE EXCEPTION 'Item not found or deleted';
        END IF;

        -- Update item entry
        UPDATE items
        SET 
            "isFavourite" = inp_isFavourite,
            modified_at = NOW(),
            modified_by = inp_userid
        WHERE item_id = inp_itemid
            AND isdelete = false;

        -- Implicit commit if no exception
    EXCEPTION WHEN OTHERS THEN
        -- Roll back transaction on error
        RAISE EXCEPTION 'Error in finding items: %', SQLERRM;
    END;
END;
$$;

-- END --

------------------------- GET MODIFIERS BY ITEM ID ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_modifiers_by_itemid_orderapp(
    inp_itemid BIGINT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
    ModifierList JSON;
BEGIN
    -- Check if the item exists and is not deleted
    IF NOT EXISTS (
        SELECT 1
        FROM items i
        WHERE i.item_id = inp_itemid AND i.isdelete = false
    ) THEN
        RETURN '[]'::JSON; -- Return empty JSON array if item not found or deleted
    END IF;

    SELECT COALESCE(json_agg(row_to_json(data)), '[]'::JSON)
    INTO ModifierList
    FROM (
        SELECT
            img.modifier_grp_id AS "ModifierGrpId",
            mg.modifier_grp_name AS "ModifierGrpName",
            img.minmodifier AS "Minmodifier",
            img.maxmodifier AS "Maxmodifier",
            (
                SELECT COALESCE(json_agg(row_to_json(modifiers)), '[]'::JSON)
                FROM (
                    SELECT
                        m.modifier_id AS "ModifierId",
                        m.modifier_name AS "ModifierName",
                        CEIL(m.rate) AS "Rate"
                    FROM modifier m
                    WHERE m.modifier_grp_id = mg.modifier_grp_id
                    AND m.isdelete = false
                    ORDER BY m.modifier_name
                ) modifiers
            ) AS "modifiersList"
        FROM "ItemModifierGroupMapping" img
        JOIN modifiergroup mg ON img.modifier_grp_id = mg.modifier_grp_id
        WHERE img.item_id = inp_itemid
        AND img.isdelete = false
        AND mg.isdelete = false
        ORDER BY mg.modifier_grp_name
    ) data;

    RETURN ModifierList;

END;
$$;

-- END --

------------------------- GET ORDER DETAIL BY CUSTOMER ID ORDERAPP -----------------------------

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