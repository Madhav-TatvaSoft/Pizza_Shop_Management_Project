------------------------- GET CUSTOMER DETAILS ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_customer_details_orderapp(
	inp_waitingid BIGINT
)
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
	CustomerList JSON;
BEGIN
	SELECT (row_to_json(t))
	INTO CustomerList
	FROM( 
		SELECT 
			w.waiting_id AS "WaitingId",
			c.customer_id AS "CustomerId",
			c.customer_name AS "CustomerName",
			c.phone_no AS "PhoneNo",
			c.email AS "Email",
			w.no_of_person AS "NoOfPerson",
			s.section_id AS "SectionId",
			s.section_name AS "SectionName"
		FROM waitinglist w
		LEFT JOIN customers c ON w.customer_id = c.customer_id
		LEFT JOIN sections s ON w.section_id = s.section_id
		WHERE w.waiting_id = inp_waitingid
		AND w.isdelete = false
		AND c.isdelete = false
		AND s.isdelete = false
		LIMIT 1
	) t;

	RETURN CustomerList;
END;
$$;

-- END --