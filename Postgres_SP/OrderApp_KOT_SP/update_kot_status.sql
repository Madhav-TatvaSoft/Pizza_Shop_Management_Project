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
    IF p_order_detail_ids IS NULL OR p_quantities IS NULL OR
       array_length(p_order_detail_ids, 1) = 0 OR
       array_length(p_quantities, 1) = 0 OR
       array_length(p_order_detail_ids, 1) != array_length(p_quantities, 1) THEN
        RAISE EXCEPTION 'Invalid input arrays: null, empty, or unequal lengths';
    END IF;

    BEGIN
        -- Loop through the arrays
        FOR i IN 1..array_length(p_order_detail_ids, 1)
        LOOP
            v_order_detail_id := p_order_detail_ids[i];
            v_quantity := p_quantities[i];

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