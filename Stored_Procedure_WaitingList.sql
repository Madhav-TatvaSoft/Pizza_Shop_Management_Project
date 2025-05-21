----------------------------------------------------------------  WAITING LIST Module --------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------

------------------------- GET SECTION LIST ORDERAPP -----------------------------

CREATE OR REPLACE FUNCTION get_section_list_orderapp()
RETURNS JSON
LANGUAGE plpgsql AS $$
DECLARE
    Sectionlist JSON;
BEGIN
    SELECT json_agg(row_to_json(list))
    INTO Sectionlist
    FROM (
        SELECT 
            s.section_id AS "SectionId",
            s.section_name AS "SectionName",
            (SELECT COUNT(*) 
             FROM tables t 
             WHERE t.section_id = s.section_id 
             AND t.status = 'Available' 
             AND t.isdelete = false) AS "AvailableCount",
            (SELECT COUNT(*) 
             FROM tables t 
             WHERE t.section_id = s.section_id 
             AND t.status IN ('Assigned', 'Occupied') 
             AND t.isdelete = false) AS "AssignedCount",
            (SELECT COUNT(*) 
             FROM tables t 
             WHERE t.section_id = s.section_id 
             AND t.status = 'Running' 
             AND t.isdelete = false) AS "RunningCount",
            (SELECT COUNT(*) 
             FROM waitinglist w 
             WHERE w.section_id = s.section_id 
             AND w.isdelete = false 
             AND w.isassign = false) AS "WaitingCount"
        FROM sections s
        WHERE s.isdelete = false
        ORDER BY s.section_id
    ) list;

    IF Sectionlist IS NULL THEN
        RETURN NULL;
    END IF;

    RETURN Sectionlist;
END;
$$;

-- END --

-------------------------------------- GET WAITING LIST ------------------------------------- 

CREATE OR REPLACE FUNCTION get_waiting_list(
---    inp_sectionid BIGINT
)
RETURNS JSON 
AS $$
DECLARE
    WaitingList JSON;
BEGIN
    SELECT COALESCE (json_agg(row_to_json(list)),'[]'::JSON)
    INTO WaitingList
    FROM (
        SELECT 
            w.waiting_id AS "WaitingId",
            w.customer_id AS "CustomerId",
            c.customer_name AS "CustomerName",
            c.phone_no AS "PhoneNo",
            c.email AS "Email",
            w.no_of_person AS "NoOfPerson",
            w.created_at AS "CreatedAt",
            w.section_id AS "SectionId",
            s.section_name AS "SectionName"
        FROM waitinglist w
        LEFT JOIN customers c ON w.customer_id = c.customer_id 
        LEFT JOIN sections s ON w.section_id = s.section_id
        WHERE w.isdelete = false 
        AND w.isassign = false
      --  AND (inp_sectionid = 0 OR w.section_id = inp_sectionid)--
        ORDER BY w.waiting_id
    ) list;

    RETURN WaitingList;
END;
$$ LANGUAGE plpgsql;

-- END -- 

-------------------------------------- DELETE TOKEN PROCEDURE -------------------------------------

CREATE OR REPLACE PROCEDURE delete_waiting_token(
    inp_waitingid BIGINT,
    inp_userid BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    -- Begin transaction
    BEGIN
        UPDATE waitinglist
        SET 
            isdelete = true,
            modified_at = now(),
            modified_by = inp_userid
        WHERE 
            waiting_id = inp_waitingid
            AND isdelete = false
            AND isassign = false;

    EXCEPTION WHEN OTHERS THEN
        -- Roll back transaction on error
        RAISE EXCEPTION 'Error in Deleting Token : %', SQLERRM;
    END;
    -- Implicit commit if no exception
END;
$$;

-- END --

-------------------------------------------- GET AVAILable TABLES -----------------------------------------

CREATE OR REPLACE FUNCTION get_available_tables(
	inp_sectionid BIGINT
)
RETURNS JSON
AS $$
DECLARE
    AvailableTableList JSON;
BEGIN
	SELECT COALESCE (json_agg(row_to_json(list)),'[]'::JSON)
	INTO AvailableTableList
	FROM (
		SELECT 
			t.table_id AS "TableId",
			t.table_name AS "TableName",
			t.section_id AS "SectionId",
			t.capacity AS "Capacity"
		FROM tables AS t
		WHERE t.section_id = inp_sectionid
		AND t.isdelete = false
		AND t.status = 'Available'
		ORDER BY t.table_id
	) list;

	RETURN AvailableTableList;
END;
$$ LANGUAGE plpgsql;

-- END --

-------------------------------------------- CHECK ALREADY ASSIGNED -----------------------------------------

CREATE OR REPLACE FUNCTION already_assigned(
	inp_customerid BIGINT
)
RETURNS BOOLEAN
AS $$
DECLARE
    IsAssigned BOOLEAN;
BEGIN
	SELECT EXISTS(
		SELECT 
		FROM "assignTable" AS at
		WHERE at.customer_id = inp_customerid
		AND at.isdelete = false
		) INTO IsAssigned;
	RETURN IsAssigned;
END;
$$ LANGUAGE plpgsql;

-- END --

-------------------------------------------- ASSIGN TABLE IN WAITING -----------------------------------------

CREATE OR REPLACE PROCEDURE assign_table_in_waiting(
    inp_waitingid BIGINT,
    inp_sectionid BIGINT,
    inp_customerid BIGINT,
    inp_persons INTEGER,
    inp_tableids INTEGER[],
    inp_userid BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_tableid INTEGER;
    v_table_exists BOOLEAN;
    v_waiting_exists BOOLEAN;
BEGIN
    -- Begin transaction
    BEGIN
        -- Check if waiting list entry exists
        SELECT EXISTS (
            SELECT 1 
            FROM waitinglist
            WHERE waiting_id = inp_waitingid 
            AND isdelete = false 
            AND isassign = false
        ) INTO v_waiting_exists;

        IF NOT v_waiting_exists THEN
            RAISE EXCEPTION 'Waiting list entry not found or already assigned/deleted';
        END IF;

        -- Update waiting list entry
        UPDATE waitinglist
        SET 
            isassign = true,
            isdelete = true,
            assigned_at = NOW(),
            modified_at = NOW(),
            modified_by = inp_userid
        WHERE waiting_id = inp_waitingid
            AND isdelete = false
            AND isassign = false;

        -- Loop through table IDs
        FOREACH v_tableid IN ARRAY inp_tableids
        LOOP
            -- Check if table exists and is available
            SELECT EXISTS (
                SELECT 1 
                FROM tables
                WHERE table_id = v_tableid 
                AND isdelete = false 
                AND status = 'Available'
            ) INTO v_table_exists;

            IF NOT v_table_exists THEN
                RAISE EXCEPTION 'Table % is not available or does not exist', v_tableid;
            END IF;

            -- Insert into assign_table
            INSERT INTO "assignTable" (
                customer_id,
                table_id,
                no_of_person,
                created_at,
                created_by
            )
            VALUES (
                inp_customerid,
                v_tableid,
                inp_persons,
                NOW(),
                inp_userid
            );

            -- Update table status
            UPDATE tables
            SET 
                status = 'Assigned',
                modified_at = NOW(),
                modified_by = inp_userid
            WHERE table_id = v_tableid
                AND isdelete = false;
        END LOOP;

        -- Implicit commit if no exception
    EXCEPTION WHEN OTHERS THEN
        -- Roll back transaction on error
        RAISE EXCEPTION 'Error in assigning table: %', SQLERRM;
    END;
END;
$$;



			


	












