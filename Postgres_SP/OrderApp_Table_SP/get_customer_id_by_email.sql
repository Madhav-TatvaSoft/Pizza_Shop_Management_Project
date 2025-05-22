
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

-- END --