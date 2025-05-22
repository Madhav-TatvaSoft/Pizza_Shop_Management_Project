----------------------------------------------------------------  OrderApp Table Module --------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------

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

-- END --

------------------------- CHECK TOKEN EXISTS ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION check_token_exists(
	inp_email VARCHAR,
	inp_waitingid BIGINT
)
RETURNS BOOLEAN
AS $$
DECLARE
    IsTokenExist BOOLEAN;
BEGIN
	SELECT EXISTS(
		SELECT 
		FROM waitinglist w
		LEFT JOIN customers c on w.customer_id = c.customer_id
		WHERE w.waiting_id != inp_waitingid
		AND c.email = inp_email
		AND w.isdelete = false
		AND w.isassign = false
		) INTO IsTokenExist;

	RETURN IsTokenExist;
END;
$$ LANGUAGE plpgsql;

-- END --

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

-------------------------  GET CUSTOMER ID BY EMAIL -----------------------------

CREATE OR REPLACE FUNCTION get_customer_id_by_email(p_email VARCHAR)
RETURNS BIGINT
LANGUAGE plpgsql AS $$
DECLARE
    v_customer_id BIGINT;
BEGIN
    SELECT customer_id
    INTO v_customer_id
    FROM customers
    WHERE email = p_email
    AND isdelete = false
    LIMIT 1;

    IF v_customer_id IS NULL THEN
        RAISE EXCEPTION 'Customer with email % not found', p_email;
    END IF;

    RETURN v_customer_id;
END;
$$;

-------------------------  SAVE CUSTOMER TO WAITING LIST  -----------------------------

CREATE OR REPLACE PROCEDURE save_customer_to_waiting_list(
    inp_waitingid BIGINT,
    inp_email VARCHAR,
    inp_noofperson INTEGER,
    inp_sectionid BIGINT,
    inp_userid BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_customer_id BIGINT;
BEGIN
    -- Begin transaction
    BEGIN
        -- Get customer_id from email
        v_customer_id := get_customer_id_by_email(inp_email);

        IF inp_waitingid = 0 THEN
            -- Insert new waitinglist record
            INSERT INTO waitinglist (
                customer_id,
                no_of_person,
                section_id,
                created_at,
                created_by,
                isdelete,
                isassign
            )
            VALUES (
                v_customer_id,
                inp_noofperson,
                inp_sectionid,
                NOW(),
                inp_userid,
                false,
                false
            );
        ELSE
            -- Update existing waitinglist record
            UPDATE waitinglist
            SET
                customer_id = v_customer_id,
                no_of_person = inp_noofperson,
                section_id = inp_sectionid,
                modified_at = NOW(),
                modified_by = inp_userid
            WHERE
                waiting_id = inp_waitingid
                AND isdelete = false
                AND isassign = false;

            IF NOT FOUND THEN
                RAISE EXCEPTION 'Waitinglist record with waiting_id % not found or already deleted/assigned', inp_waitingid;
            END IF;
        END IF;

    EXCEPTION WHEN OTHERS THEN
        -- Roll back transaction on error
        RAISE EXCEPTION 'Error in saving customer to waiting list: %', SQLERRM;
    END;
    -- Implicit commit if no exception
END;
$$;

-- END -- 

-------------------------  ASSIGN TABLE IN ORDERAPP_TABLE  -----------------------------

CREATE OR REPLACE PROCEDURE assign_table(
    inp_waitingid BIGINT,
    inp_email VARCHAR,
    inp_noofperson INTEGER,
    inp_tableids JSON,
    inp_userid BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_customer_id BIGINT;
    v_table_id BIGINT;
    v_table_count INTEGER;
BEGIN
    -- Begin transaction
    BEGIN
        -- Get customer_id from email
        v_customer_id := get_customer_id_by_email(inp_email);

        -- Update waitinglist record
        UPDATE waitinglist
        SET
            isassign = TRUE,
            assigned_at = NOW(),
            modified_at = NOW(),
            modified_by = inp_userid
        WHERE
            waiting_id = inp_waitingid
            AND isdelete = FALSE
            AND isassign = FALSE;

        IF NOT FOUND THEN
            RAISE EXCEPTION 'Waitinglist record with waiting_id % not found or already deleted/assigned', inp_waitingid;
        END IF;

        -- Validate table_ids array
        SELECT COUNT(*) INTO v_table_count
        FROM json_array_elements_text(inp_tableids) AS t(id);

        IF v_table_count = 0 THEN
            RAISE EXCEPTION 'Table IDs array is empty';
        END IF;

        -- Check if all tables are available
        SELECT COUNT(*) INTO v_table_count
        FROM tables t
        WHERE t.table_id IN (SELECT CAST(json_array_elements_text(inp_tableids) AS BIGINT))
        AND t.isdelete = FALSE
        AND t.status = 'Available';

        IF v_table_count != (SELECT json_array_length(inp_tableids)) THEN
            RAISE EXCEPTION 'One or more tables are not available or deleted';
        END IF;

        -- Insert assign_table records and update tables
        FOR v_table_id IN
            SELECT CAST(json_array_elements_text(inp_tableids) AS BIGINT)
        LOOP
            -- Insert into assign_table
            INSERT INTO assign_table (
                customer_id,
                table_id,
                no_of_person,
                created_at,
                created_by
            )
            VALUES (
                v_customer_id,
                v_table_id,
                inp_noofperson,
                NOW(),
                inp_userid
            );

            -- Update table status
            UPDATE tables
            SET
                status = 'Assigned',
                modified_at = NOW(),
                modified_by = inp_userid
            WHERE
                table_id = v_table_id
                AND isdelete = FALSE;
        END LOOP;

    EXCEPTION WHEN OTHERS THEN
        -- Roll back transaction on error
        RAISE EXCEPTION 'Error in assigning table: %', SQLERRM;
    END;
    -- Implicit commit if no exception
END;
$$;

DROP PROCEDURE assign_table