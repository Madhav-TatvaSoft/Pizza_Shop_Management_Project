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