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