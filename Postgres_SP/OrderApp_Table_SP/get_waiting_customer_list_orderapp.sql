------------------------- GET WAITING CUSTOMER LIST ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_waiting_customer_list_orderapp(
	inp_sectionid BIGINT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
	CustomerList JSON;
BEGIN
	SELECT COALESCE (json_agg(row_to_json(list)),'[]'::JSON)
	INTO CustomerList
	FROM( 
		SELECT 
			w.waiting_id AS "WaitingId",
			c.customer_name AS "CustomerName",
			c.phone_no AS "PhoneNo",
			c.email AS "Email",
			w.no_of_person AS "NoOfPerson",
			s.section_id AS "SectionId",
			s.section_name AS "SectionName"
		FROM waitinglist w
		LEFT JOIN customers c ON w.customer_id = c.customer_id
		LEFT JOIN sections s ON s.section_id = w.section_id
		WHERE s.section_id = inp_sectionid
		AND w.isdelete = false
		AND w.isassign = false
		AND c.isdelete = false
		AND s.isdelete = false
		ORDER BY w.waiting_id
	) list;

	IF CustomerList IS NULL THEN
		RETURN NULL;
	END IF;

	RETURN CustomerList;
END;
$$;

-- END --