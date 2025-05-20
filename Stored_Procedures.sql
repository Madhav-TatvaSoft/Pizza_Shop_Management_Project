----------------------------------------------------------------  KOT Module --------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------------------

-- UPDATE KOT STATUS Procedure -- 

CREATE OR REPLACE PROCEDURE update_kot_status(
    IN p_filter VARCHAR,
    IN p_order_detail_ids BIGINT[],
    IN p_quantities INTEGER[]
)
LANGUAGE plpgsql
AS $$
DECLARE
    i INTEGER;
    v_order_detail_id BIGINT;
    v_quantity INTEGER;
    v_rows_affected INTEGER;
BEGIN
    -- Validating the input arrays
    IF p_order_detail_ids IS NULL OR p_quantities IS NULL OR
       array_length(p_order_detail_ids, 1) = 0 OR
       array_length(p_quantities, 1) = 0 OR
       array_length(p_order_detail_ids, 1) != array_length(p_quantities, 1) THEN
        RAISE EXCEPTION 'Invalid input arrays: null, empty, or unequal lengths';
    END IF;

    -- Begin transaction
    BEGIN
        -- Loop through the arrays
        FOR i IN 1..array_length(p_order_detail_ids, 1)
        LOOP
            v_order_detail_id := p_order_detail_ids[i];
            v_quantity := p_quantities[i];

            -- Update based on filter
            IF p_filter = 'In_Progress' THEN
                UPDATE orderdetails
                SET "readyQuantity" = "readyQuantity" + v_quantity
                WHERE orderdetail_id = v_order_detail_id
                AND isdelete = FALSE
                AND status NOT IN ('Completed', 'Cancelled');
            ELSE
                UPDATE orderdetails
                SET "readyQuantity" = "readyQuantity" - v_quantity
                WHERE orderdetail_id = v_order_detail_id
                AND isdelete = FALSE
                AND status NOT IN ('Completed', 'Cancelled');
            END IF;

            -- Check if update affected any rows or not
            GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
            IF v_rows_affected = 0 THEN
                RAISE EXCEPTION 'No valid order detail found for orderdetail_id %', v_order_detail_id;
            END IF;
        END LOOP;

    EXCEPTION WHEN OTHERS THEN
        -- Re-raise the exception to trigger rollback
        RAISE;
    END;
END;
$$;

DROP PROCEDURE update_kot_status(character varying,bigint[],integer[])

-- END Procedure --

-- GET KOT ITEMS FUNCTION --

CREATE OR REPLACE FUNCTION get_kot_items(
    p_catid BIGINT,
    p_filter VARCHAR
) RETURNS JSON
LANGUAGE plpgsql
AS $$
DECLARE
    v_json JSON;
BEGIN
    SELECT json_agg(t.order_json) INTO v_json
    FROM (
        SELECT json_build_object(
            'categoryList', COALESCE((
                SELECT json_agg(json_build_object(
                    'categoryId', c.category_id,
                    'categoryName', c.category_name
                ))
                FROM orderdetails od
                JOIN items i ON od.item_id = i.item_id
                JOIN category c ON i.category_id = c.category_id
                WHERE od.order_id = o.order_id
                AND NOT od.isdelete
                AND NOT i.isdelete
                AND NOT c.isdelete
                AND (p_catid = 0 OR i.category_id = p_catid)
                AND (p_filter = 'Ready' AND od."readyQuantity" > 0 OR
                     p_filter != 'Ready' AND od.quantity - od."readyQuantity" > 0)
            ), '[]'),
            'orderId', o.order_id,
            'extraInstruction', COALESCE(o.extra_instruction, ''),
            'orderDate', o.order_date,
            'tableList', COALESCE((
                SELECT json_agg(json_build_object(
                    'tableId', t.table_id,
                    'tableName', t.table_name
                ))
                FROM "assignTable" at
                JOIN tables t ON at.table_id = t.table_id
                WHERE at.order_id = o.order_id
                AND NOT t.isdelete
            ), '[]'),
            'sectionId', o.section_id,
            'sectionName', COALESCE(s.section_name, ''),
            'itemOrderVM', COALESCE((
                SELECT json_agg(json_build_object(
                    'itemId', od.item_id,
                    'orderdetailId', od.orderdetail_id,
                    'itemName', i.item_name,
                    'extraInstruction', COALESCE(od.extra_instruction, ''),
                    'quantity', CASE
                        WHEN p_filter = 'Ready' THEN od."readyQuantity"
                        ELSE od.quantity - od."readyQuantity"
                    END,
                    'modifierOrderVM', COALESCE((
                        SELECT json_agg(json_build_object(
                            'modifierId', mo.modifier_id,
                            'modifierName', m.modifier_name
                        ))
                        FROM modifierorder mo
                        JOIN modifier m ON mo.modifier_id = m.modifier_id
                        WHERE mo.orderdetail_id = od.orderdetail_id
                        AND NOT m.isdelete
                    ), '[]')
                ))
                FROM orderdetails od
                JOIN items i ON od.item_id = i.item_id
                WHERE od.order_id = o.order_id
                AND NOT od.isdelete
                AND NOT i.isdelete
                AND (p_catid = 0 OR i.category_id = p_catid)
                AND (p_filter = 'Ready' AND od."readyQuantity" > 0 OR
                     p_filter != 'Ready' AND od.quantity - od."readyQuantity" > 0)
            ), '[]')
        ) AS order_json
        FROM orders o
        JOIN sections s ON o.section_id = s.section_id
        WHERE NOT o.isdelete
        AND o.status NOT IN ('Completed', 'Cancelled')
        AND EXISTS (
            SELECT 1
            FROM orderdetails od
            JOIN items i ON od.item_id = i.item_id
            WHERE od.order_id = o.order_id
            AND NOT od.isdelete
            AND NOT i.isdelete
            AND (p_catid = 0 OR i.category_id = p_catid)
            AND (p_filter = 'Ready' AND od."readyQuantity" > 0 OR
                 p_filter != 'Ready' AND od.quantity - od."readyQuantity" > 0)
        )
        ORDER BY o.order_id DESC
    ) t;

    RETURN COALESCE(v_json, '[]'::JSON);
END;
$$;

-- END --

----------------------------------------------------------------  WAITING LIST Module --------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------

-- GET WAITING TOKEN -- 

CREATE OR REPLACE FUNCTION get_waiting_list(
	inp_sectionid BIGINT
)
RETURNS JSON 
LANGUAGE plpgsql AS $$
DECLARE WaitingList JSON;
BEGIN
	select Json_agg(row_to_json(list)) into Waitinglist
	from(
		select 
			w.waiting_id as "WaitingId",
			w.created_at as "CreatedAt",
			w.customer_id as "CustomerId",
			c.customer_name as "CustomerName",
			w.no_of_person as "NoOfPerson",
			c.phone_no as "PhoneNo",
			c.email as "Email",
			w.created_at as "WaitingTime",
			w.section_id as "SectionId",
			s.section_name as "SectionName"
			from waitinglist as w
			left join customers as c on w.customer_id = c.customer_id
			left join sections as s on w.section_id = s.section_id
			where w.isdelete = false AND w.isassign = false 
			-- case 
			-- 	when inputSectionId = 0 then AND w.isdelete = false
			-- 	else AND w.section_id = inputSectionId
			-- end
			order By w.waiting_id
	)list;
	RETURN Waitinglist;
	END;
$$;









