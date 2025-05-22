------------------------- GET TABLE LIST BY SECTION ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_tables_by_section_orderapp(
	inp_sectionid BIGINT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
	TableList JSON;
BEGIN
	SELECT COALESCE (json_agg(row_to_json(list)),'[]'::JSON)
	INTO TableList
	FROM( 
		SELECT 
			t.table_id AS "TableId",
			t.table_name AS "TableName",
			t.section_id AS "SectionId",
			t.capacity AS "Capacity",
			t.status AS "Status",
			COALESCE(
                (SELECT at.created_at 
                 FROM "assignTable" at 
                 WHERE at.table_id = t.table_id 
                 AND at.isdelete = false 
                 ORDER BY at.created_at 
                 LIMIT 1),
                NOW()
            ) AS "TableTime",
            CASE 
                WHEN t.status = 'Running' THEN
                    COALESCE(
                        (SELECT o.total_amount 
                         FROM "assignTable" at 
                         LEFT JOIN orders o ON at.order_id = o.order_id
	                     WHERE at.table_id = t.table_id
                         AND at.isdelete = false 
                         ORDER BY at.created_at 
                         LIMIT 1),
                        0
                    )
                ELSE 0
            END AS "OrderAmount"
		FROM tables t
		WHERE t.section_id = inp_sectionid
		AND t.isdelete = false
		ORDER BY t.table_id
	) list;

	IF TableList IS NULL THEN
		RETURN NULL;
	END IF;

	RETURN TableList;
END;
$$;