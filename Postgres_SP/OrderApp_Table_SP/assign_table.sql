-------------------------  ASSIGN TABLE IN ORDERAPP_TABLE  -----------------------------

CREATE OR REPLACE PROCEDURE assign_table(
    inp_waitingid BIGINT,
    inp_email VARCHAR,
    inp_noofperson INTEGER,
    inp_tableids INTEGER[],
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
        IF array_length(inp_tableids, 1) IS NULL THEN
            RAISE EXCEPTION 'Table IDs array is empty';
        END IF;

        -- Check if all tables are available
        SELECT COUNT(*) INTO v_table_count
        FROM tables t
        WHERE t.table_id = ANY(inp_tableids)
        AND t.isdelete = FALSE
        AND t.status = 'Available';

        IF v_table_count != array_length(inp_tableids, 1) THEN
            RAISE EXCEPTION 'One or more tables are not available or deleted';
        END IF;

        -- Insert assign_table records and update tables
        FOREACH v_table_id IN ARRAY inp_tableids
        LOOP
            -- Insert into assign_table
            INSERT INTO "assignTable" (
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

-- END --