----------------------------------------------------------------  WAITING LIST Module --------------------------------------------------------------
----------------------------------------------------------------------------------------------------------------------------------------------------

----------- GET SECTION LIST ORDERAPP -------------

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

---------------- GET WAITING LIST -------------- 

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
$$ LANGUAGE plpgsql ;

-- END -- 

-- DELETE TOKEN PROCEDURE --

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
