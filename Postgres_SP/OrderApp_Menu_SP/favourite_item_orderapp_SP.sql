------------------------- FAVOURITE ITEM ORDERAPP -----------------------------

CREATE OR REPLACE PROCEDURE favourite_item_orderapp(
	inp_itemid BIGINT,
	inp_isFavourite BOOLEAN,
	inp_userid BIGINT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_item_exists BOOLEAN;
BEGIN
    -- Begin transaction
    BEGIN
        -- Check if item exists
        SELECT EXISTS (
            SELECT 1 
            FROM items
            WHERE item_id = inp_itemid 
            AND isdelete = false 
        ) INTO v_item_exists;

        IF NOT v_item_exists THEN
            RAISE EXCEPTION 'Item not found or deleted';
        END IF;

        -- Update item entry
        UPDATE items
        SET 
            "isFavourite" = inp_isFavourite,
            modified_at = NOW(),
            modified_by = inp_userid
        WHERE item_id = inp_itemid
            AND isdelete = false;

        -- Implicit commit if no exception
    EXCEPTION WHEN OTHERS THEN
        -- Roll back transaction on error
        RAISE EXCEPTION 'Error in finding items: %', SQLERRM;
    END;
END;
$$;

-- END --