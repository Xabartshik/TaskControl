--
-- PostgreSQL database dump
--

\restrict jn0PGtGNWLbEMY40x1rjTU7mSsIoPKCju1hSc14Y1xEfAckiaNi4lBlREDX1jJ4

-- Dumped from database version 15.15 (Debian 15.15-1.pgdg13+1)
-- Dumped by pg_dump version 15.15 (Debian 15.15-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: active_assigned_tasks; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.active_assigned_tasks (
    id integer NOT NULL,
    task_id integer NOT NULL,
    user_id integer NOT NULL,
    assigned_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone
);


ALTER TABLE public.active_assigned_tasks OWNER TO taskservice_user;

--
-- Name: active_assigned_tasks_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.active_assigned_tasks_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.active_assigned_tasks_id_seq OWNER TO taskservice_user;

--
-- Name: active_assigned_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.active_assigned_tasks_id_seq OWNED BY public.active_assigned_tasks.id;


--
-- Name: base_tasks; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.base_tasks (
    task_id integer NOT NULL,
    title character varying(200) NOT NULL,
    description character varying(2000),
    branch_id integer NOT NULL,
    type character varying(50) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at timestamp without time zone,
    status character varying(20) DEFAULT 'New'::character varying NOT NULL,
    priority integer DEFAULT 5 NOT NULL,
    CONSTRAINT base_tasks_priority_check CHECK (((priority >= 0) AND (priority <= 10))),
    CONSTRAINT base_tasks_status_check CHECK (((status)::text = ANY ((ARRAY['New'::character varying, 'Assigned'::character varying, 'InProgress'::character varying, 'Completed'::character varying, 'Cancelled'::character varying, 'OnHold'::character varying, 'Blocked'::character varying])::text[])))
);


ALTER TABLE public.base_tasks OWNER TO taskservice_user;

--
-- Name: base_tasks_task_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.base_tasks_task_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.base_tasks_task_id_seq OWNER TO taskservice_user;

--
-- Name: base_tasks_task_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.base_tasks_task_id_seq OWNED BY public.base_tasks.task_id;


--
-- Name: branches; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.branches (
    branch_id integer NOT NULL,
    branch_name character varying(200) NOT NULL,
    branch_type character varying(100) NOT NULL,
    address character varying(500) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.branches OWNER TO taskservice_user;

--
-- Name: branches_branch_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.branches_branch_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.branches_branch_id_seq OWNER TO taskservice_user;

--
-- Name: branches_branch_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.branches_branch_id_seq OWNED BY public.branches.branch_id;


--
-- Name: check_io_employees; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.check_io_employees (
    id integer NOT NULL,
    employee_id integer NOT NULL,
    branch_id integer NOT NULL,
    check_type character varying(3) NOT NULL,
    check_timestamp timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT check_io_employees_check_type_check CHECK (((check_type)::text = ANY ((ARRAY['in'::character varying, 'out'::character varying])::text[])))
);


ALTER TABLE public.check_io_employees OWNER TO taskservice_user;

--
-- Name: check_io_employees_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.check_io_employees_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.check_io_employees_id_seq OWNER TO taskservice_user;

--
-- Name: check_io_employees_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.check_io_employees_id_seq OWNED BY public.check_io_employees.id;


--
-- Name: employees; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.employees (
    employees_id integer NOT NULL,
    surname character varying(100) NOT NULL,
    name character varying(100) NOT NULL,
    middle_name character varying(100),
    role character varying(150) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.employees OWNER TO taskservice_user;

--
-- Name: employees_employees_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.employees_employees_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.employees_employees_id_seq OWNER TO taskservice_user;

--
-- Name: employees_employees_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.employees_employees_id_seq OWNED BY public.employees.employees_id;


--
-- Name: inventory_assignment_lines; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.inventory_assignment_lines (
    id integer NOT NULL,
    inventory_assignment_id integer NOT NULL,
    item_position_id integer NOT NULL,
    position_id integer NOT NULL,
    expected_quantity integer NOT NULL,
    actual_quantity integer,
    zone_code character varying(10) NOT NULL,
    first_level_storage_type character varying(30) NOT NULL,
    fls_number character varying(20) NOT NULL,
    second_level_storage character varying(30),
    third_level_storage character varying(30),
    CONSTRAINT inventory_assignment_lines_expected_quantity_check CHECK ((expected_quantity >= 0))
);


ALTER TABLE public.inventory_assignment_lines OWNER TO taskservice_user;

--
-- Name: inventory_assignment_lines_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.inventory_assignment_lines_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.inventory_assignment_lines_id_seq OWNER TO taskservice_user;

--
-- Name: inventory_assignment_lines_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_assignment_lines_id_seq OWNED BY public.inventory_assignment_lines.id;


--
-- Name: inventory_assignments; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.inventory_assignments (
    id integer NOT NULL,
    task_id integer NOT NULL,
    assigned_to_user_id integer NOT NULL,
    branch_id integer NOT NULL,
    status integer NOT NULL,
    assigned_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at timestamp without time zone,
    CONSTRAINT inventory_assignments_status_check CHECK ((status = ANY (ARRAY[0, 1, 2, 3])))
);


ALTER TABLE public.inventory_assignments OWNER TO taskservice_user;

--
-- Name: inventory_assignments_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.inventory_assignments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.inventory_assignments_id_seq OWNER TO taskservice_user;

--
-- Name: inventory_assignments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_assignments_id_seq OWNED BY public.inventory_assignments.id;


--
-- Name: inventory_discrepancies; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.inventory_discrepancies (
    id integer NOT NULL,
    inventory_assignment_line_id integer NOT NULL,
    item_position_id integer NOT NULL,
    expected_quantity integer NOT NULL,
    actual_quantity integer NOT NULL,
    type integer NOT NULL,
    note text,
    identified_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    resolution_status integer NOT NULL,
    resolved_at timestamp without time zone,
    CONSTRAINT inventory_discrepancies_actual_quantity_check CHECK ((actual_quantity >= 0)),
    CONSTRAINT inventory_discrepancies_expected_quantity_check CHECK ((expected_quantity >= 0)),
    CONSTRAINT inventory_discrepancies_resolution_status_check CHECK ((resolution_status = ANY (ARRAY[0, 1, 2, 3]))),
    CONSTRAINT inventory_discrepancies_type_check CHECK ((type = ANY (ARRAY[0, 1, 2]))),
    CONSTRAINT positive_variance CHECK (((actual_quantity - expected_quantity) IS NOT NULL))
);


ALTER TABLE public.inventory_discrepancies OWNER TO taskservice_user;

--
-- Name: inventory_discrepancies_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.inventory_discrepancies_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.inventory_discrepancies_id_seq OWNER TO taskservice_user;

--
-- Name: inventory_discrepancies_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_discrepancies_id_seq OWNED BY public.inventory_discrepancies.id;


--
-- Name: inventory_statistics; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.inventory_statistics (
    id integer NOT NULL,
    inventory_assignment_id integer NOT NULL,
    total_positions integer NOT NULL,
    counted_positions integer NOT NULL,
    discrepancy_count integer NOT NULL,
    surplus_count integer NOT NULL,
    shortage_count integer NOT NULL,
    total_surplus_quantity integer NOT NULL,
    total_shortage_quantity integer NOT NULL,
    started_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at timestamp without time zone,
    CONSTRAINT inventory_statistics_counted_positions_check CHECK ((counted_positions >= 0)),
    CONSTRAINT inventory_statistics_discrepancy_count_check CHECK ((discrepancy_count >= 0)),
    CONSTRAINT inventory_statistics_shortage_count_check CHECK ((shortage_count >= 0)),
    CONSTRAINT inventory_statistics_surplus_count_check CHECK ((surplus_count >= 0)),
    CONSTRAINT inventory_statistics_total_positions_check CHECK ((total_positions > 0)),
    CONSTRAINT inventory_statistics_total_shortage_quantity_check CHECK ((total_shortage_quantity >= 0)),
    CONSTRAINT inventory_statistics_total_surplus_quantity_check CHECK ((total_surplus_quantity >= 0))
);


ALTER TABLE public.inventory_statistics OWNER TO taskservice_user;

--
-- Name: inventory_statistics_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.inventory_statistics_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.inventory_statistics_id_seq OWNER TO taskservice_user;

--
-- Name: inventory_statistics_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_statistics_id_seq OWNED BY public.inventory_statistics.id;


--
-- Name: item_movements; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.item_movements (
    id integer NOT NULL,
    source_item_position_id integer,
    destination_position_id integer,
    source_branch_id integer,
    destination_branch_id integer,
    quantity integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT item_movements_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public.item_movements OWNER TO taskservice_user;

--
-- Name: item_movements_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.item_movements_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.item_movements_id_seq OWNER TO taskservice_user;

--
-- Name: item_movements_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.item_movements_id_seq OWNED BY public.item_movements.id;


--
-- Name: item_positions; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.item_positions (
    id integer NOT NULL,
    item_id integer NOT NULL,
    position_id integer NOT NULL,
    quantity integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT item_positions_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public.item_positions OWNER TO taskservice_user;

--
-- Name: item_positions_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.item_positions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.item_positions_id_seq OWNER TO taskservice_user;

--
-- Name: item_positions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.item_positions_id_seq OWNED BY public.item_positions.id;


--
-- Name: item_statuses; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.item_statuses (
    id integer NOT NULL,
    item_position_id integer NOT NULL,
    status character varying(20) NOT NULL,
    status_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    quantity integer NOT NULL,
    CONSTRAINT item_statuses_quantity_check CHECK ((quantity > 0)),
    CONSTRAINT item_statuses_status_check CHECK (((status)::text = ANY ((ARRAY['Available'::character varying, 'Reserved'::character varying, 'Shipped'::character varying, 'Defective'::character varying])::text[])))
);


ALTER TABLE public.item_statuses OWNER TO taskservice_user;

--
-- Name: item_statuses_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.item_statuses_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.item_statuses_id_seq OWNER TO taskservice_user;

--
-- Name: item_statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.item_statuses_id_seq OWNED BY public.item_statuses.id;


--
-- Name: items; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.items (
    item_id integer NOT NULL,
    name character varying(100) NOT NULL,
    weight double precision NOT NULL,
    length double precision NOT NULL,
    width double precision NOT NULL,
    height double precision NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT items_height_check CHECK ((height > (0)::double precision)),
    CONSTRAINT items_length_check CHECK ((length > (0)::double precision)),
    CONSTRAINT items_weight_check CHECK ((weight > (0)::double precision)),
    CONSTRAINT items_width_check CHECK ((width > (0)::double precision))
);


ALTER TABLE public.items OWNER TO taskservice_user;

--
-- Name: items_item_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.items_item_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.items_item_id_seq OWNER TO taskservice_user;

--
-- Name: items_item_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.items_item_id_seq OWNED BY public.items.item_id;


--
-- Name: mobile_app_users; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.mobile_app_users (
    id integer NOT NULL,
    employee_id integer NOT NULL,
    password_hash text NOT NULL,
    role character varying(30) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp without time zone,
    branch_id integer
);


ALTER TABLE public.mobile_app_users OWNER TO taskservice_user;

--
-- Name: mobile_app_users_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.mobile_app_users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.mobile_app_users_id_seq OWNER TO taskservice_user;

--
-- Name: mobile_app_users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.mobile_app_users_id_seq OWNED BY public.mobile_app_users.id;


--
-- Name: order_positions; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.order_positions (
    unique_id integer NOT NULL,
    order_id integer NOT NULL,
    item_position_id integer NOT NULL,
    quantity integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT order_positions_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public.order_positions OWNER TO taskservice_user;

--
-- Name: order_positions_unique_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.order_positions_unique_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_positions_unique_id_seq OWNER TO taskservice_user;

--
-- Name: order_positions_unique_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.order_positions_unique_id_seq OWNED BY public.order_positions.unique_id;


--
-- Name: orders; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.orders (
    order_id integer NOT NULL,
    customer_id integer NOT NULL,
    branch_id integer NOT NULL,
    delivery_date timestamp without time zone,
    type character varying(20) NOT NULL,
    status character varying(20) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT orders_status_check CHECK (((status)::text = ANY ((ARRAY['New'::character varying, 'Processing'::character varying, 'Shipped'::character varying, 'Delivered'::character varying, 'Cancelled'::character varying])::text[]))),
    CONSTRAINT orders_type_check CHECK (((type)::text = ANY ((ARRAY['Online'::character varying, 'Offline'::character varying, 'Wholesale'::character varying])::text[])))
);


ALTER TABLE public.orders OWNER TO taskservice_user;

--
-- Name: orders_order_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.orders_order_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.orders_order_id_seq OWNER TO taskservice_user;

--
-- Name: orders_order_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.orders_order_id_seq OWNED BY public.orders.order_id;


--
-- Name: positions; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.positions (
    position_id integer NOT NULL,
    branch_id integer NOT NULL,
    status character varying(20) NOT NULL,
    zone_code character varying(10) NOT NULL,
    first_level_storage_type character varying(30) NOT NULL,
    fls_number character varying(20) NOT NULL,
    second_level_storage character varying(30),
    third_level_storage character varying(30),
    length double precision,
    width double precision,
    height double precision,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT positions_height_check CHECK ((height > (0)::double precision)),
    CONSTRAINT positions_length_check CHECK ((length > (0)::double precision)),
    CONSTRAINT positions_status_check CHECK (((status)::text = ANY ((ARRAY['Active'::character varying, 'Inactive'::character varying, 'Maintenance'::character varying])::text[]))),
    CONSTRAINT positions_width_check CHECK ((width > (0)::double precision))
);


ALTER TABLE public.positions OWNER TO taskservice_user;

--
-- Name: positions_position_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.positions_position_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.positions_position_id_seq OWNER TO taskservice_user;

--
-- Name: positions_position_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.positions_position_id_seq OWNED BY public.positions.position_id;


--
-- Name: raw_events; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.raw_events (
    report_id integer NOT NULL,
    type character varying(50) NOT NULL,
    json_params jsonb NOT NULL,
    event_time timestamp without time zone NOT NULL,
    source_service character varying(100) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.raw_events OWNER TO taskservice_user;

--
-- Name: raw_events_report_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.raw_events_report_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.raw_events_report_id_seq OWNER TO taskservice_user;

--
-- Name: raw_events_report_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.raw_events_report_id_seq OWNED BY public.raw_events.report_id;


--
-- Name: active_assigned_tasks id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks ALTER COLUMN id SET DEFAULT nextval('public.active_assigned_tasks_id_seq'::regclass);


--
-- Name: base_tasks task_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.base_tasks ALTER COLUMN task_id SET DEFAULT nextval('public.base_tasks_task_id_seq'::regclass);


--
-- Name: branches branch_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.branches ALTER COLUMN branch_id SET DEFAULT nextval('public.branches_branch_id_seq'::regclass);


--
-- Name: check_io_employees id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees ALTER COLUMN id SET DEFAULT nextval('public.check_io_employees_id_seq'::regclass);


--
-- Name: employees employees_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.employees ALTER COLUMN employees_id SET DEFAULT nextval('public.employees_employees_id_seq'::regclass);


--
-- Name: inventory_assignment_lines id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines ALTER COLUMN id SET DEFAULT nextval('public.inventory_assignment_lines_id_seq'::regclass);


--
-- Name: inventory_assignments id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments ALTER COLUMN id SET DEFAULT nextval('public.inventory_assignments_id_seq'::regclass);


--
-- Name: inventory_discrepancies id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies ALTER COLUMN id SET DEFAULT nextval('public.inventory_discrepancies_id_seq'::regclass);


--
-- Name: inventory_statistics id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics ALTER COLUMN id SET DEFAULT nextval('public.inventory_statistics_id_seq'::regclass);


--
-- Name: item_movements id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements ALTER COLUMN id SET DEFAULT nextval('public.item_movements_id_seq'::regclass);


--
-- Name: item_positions id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions ALTER COLUMN id SET DEFAULT nextval('public.item_positions_id_seq'::regclass);


--
-- Name: item_statuses id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_statuses ALTER COLUMN id SET DEFAULT nextval('public.item_statuses_id_seq'::regclass);


--
-- Name: items item_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.items ALTER COLUMN item_id SET DEFAULT nextval('public.items_item_id_seq'::regclass);


--
-- Name: mobile_app_users id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users ALTER COLUMN id SET DEFAULT nextval('public.mobile_app_users_id_seq'::regclass);


--
-- Name: order_positions unique_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions ALTER COLUMN unique_id SET DEFAULT nextval('public.order_positions_unique_id_seq'::regclass);


--
-- Name: orders order_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders ALTER COLUMN order_id SET DEFAULT nextval('public.orders_order_id_seq'::regclass);


--
-- Name: positions position_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.positions ALTER COLUMN position_id SET DEFAULT nextval('public.positions_position_id_seq'::regclass);


--
-- Name: raw_events report_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.raw_events ALTER COLUMN report_id SET DEFAULT nextval('public.raw_events_report_id_seq'::regclass);


--
-- Name: active_assigned_tasks active_assigned_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks
    ADD CONSTRAINT active_assigned_tasks_pkey PRIMARY KEY (id);


--
-- Name: base_tasks base_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.base_tasks
    ADD CONSTRAINT base_tasks_pkey PRIMARY KEY (task_id);


--
-- Name: branches branches_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.branches
    ADD CONSTRAINT branches_pkey PRIMARY KEY (branch_id);


--
-- Name: check_io_employees check_io_employees_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees
    ADD CONSTRAINT check_io_employees_pkey PRIMARY KEY (id);


--
-- Name: employees employees_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_pkey PRIMARY KEY (employees_id);


--
-- Name: inventory_assignment_lines inventory_assignment_lines_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_pkey PRIMARY KEY (id);


--
-- Name: inventory_assignments inventory_assignments_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_pkey PRIMARY KEY (id);


--
-- Name: inventory_discrepancies inventory_discrepancies_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies
    ADD CONSTRAINT inventory_discrepancies_pkey PRIMARY KEY (id);


--
-- Name: inventory_statistics inventory_statistics_inventory_assignment_id_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics
    ADD CONSTRAINT inventory_statistics_inventory_assignment_id_key UNIQUE (inventory_assignment_id);


--
-- Name: inventory_statistics inventory_statistics_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics
    ADD CONSTRAINT inventory_statistics_pkey PRIMARY KEY (id);


--
-- Name: item_movements item_movements_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_pkey PRIMARY KEY (id);


--
-- Name: item_positions item_positions_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions
    ADD CONSTRAINT item_positions_pkey PRIMARY KEY (id);


--
-- Name: item_statuses item_statuses_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_statuses
    ADD CONSTRAINT item_statuses_pkey PRIMARY KEY (id);


--
-- Name: items items_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_pkey PRIMARY KEY (item_id);


--
-- Name: mobile_app_users mobile_app_users_employee_id_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users
    ADD CONSTRAINT mobile_app_users_employee_id_key UNIQUE (employee_id);


--
-- Name: mobile_app_users mobile_app_users_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users
    ADD CONSTRAINT mobile_app_users_pkey PRIMARY KEY (id);


--
-- Name: order_positions order_positions_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions
    ADD CONSTRAINT order_positions_pkey PRIMARY KEY (unique_id);


--
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (order_id);


--
-- Name: positions positions_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT positions_pkey PRIMARY KEY (position_id);


--
-- Name: raw_events raw_events_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.raw_events
    ADD CONSTRAINT raw_events_pkey PRIMARY KEY (report_id);


--
-- Name: idx_assigned_tasks_combo; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_assigned_tasks_combo ON public.active_assigned_tasks USING btree (task_id, user_id);


--
-- Name: idx_assigned_tasks_user; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_assigned_tasks_user ON public.active_assigned_tasks USING btree (user_id);


--
-- Name: idx_base_tasks_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_branch ON public.base_tasks USING btree (branch_id);


--
-- Name: idx_base_tasks_completed; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_completed ON public.base_tasks USING btree (completed_at);


--
-- Name: idx_base_tasks_created; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_created ON public.base_tasks USING btree (created_at);


--
-- Name: idx_base_tasks_priority; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_priority ON public.base_tasks USING btree (priority);


--
-- Name: idx_base_tasks_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_status ON public.base_tasks USING btree (status);


--
-- Name: idx_base_tasks_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_type ON public.base_tasks USING btree (type);


--
-- Name: idx_branches_name; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE UNIQUE INDEX idx_branches_name ON public.branches USING btree (branch_name);


--
-- Name: idx_branches_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_branches_type ON public.branches USING btree (branch_type);


--
-- Name: idx_check_io_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_branch ON public.check_io_employees USING btree (branch_id);


--
-- Name: idx_check_io_employee; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_employee ON public.check_io_employees USING btree (employee_id);


--
-- Name: idx_check_io_timestamp; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_timestamp ON public.check_io_employees USING btree (check_timestamp);


--
-- Name: idx_check_io_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_type ON public.check_io_employees USING btree (check_type);


--
-- Name: idx_discrepancies_identified; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_identified ON public.inventory_discrepancies USING btree (identified_at);


--
-- Name: idx_discrepancies_itemposition; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_itemposition ON public.inventory_discrepancies USING btree (item_position_id);


--
-- Name: idx_discrepancies_line; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_line ON public.inventory_discrepancies USING btree (inventory_assignment_line_id);


--
-- Name: idx_discrepancies_resolution; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_resolution ON public.inventory_discrepancies USING btree (resolution_status);


--
-- Name: idx_discrepancies_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_type ON public.inventory_discrepancies USING btree (type);


--
-- Name: idx_employees_name; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_employees_name ON public.employees USING btree (surname, name);


--
-- Name: idx_employees_role; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_employees_role ON public.employees USING btree (role);


--
-- Name: idx_inventory_assignments_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_branch ON public.inventory_assignments USING btree (branch_id);


--
-- Name: idx_inventory_assignments_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_status ON public.inventory_assignments USING btree (status);


--
-- Name: idx_inventory_assignments_task; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_task ON public.inventory_assignments USING btree (task_id);


--
-- Name: idx_inventory_assignments_user; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_user ON public.inventory_assignments USING btree (assigned_to_user_id);


--
-- Name: idx_inventory_lines_assignment; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_assignment ON public.inventory_assignment_lines USING btree (inventory_assignment_id);


--
-- Name: idx_inventory_lines_itemposition; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_itemposition ON public.inventory_assignment_lines USING btree (item_position_id);


--
-- Name: idx_inventory_lines_position; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_position ON public.inventory_assignment_lines USING btree (position_id);


--
-- Name: idx_inventory_lines_zone; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_zone ON public.inventory_assignment_lines USING btree (zone_code);


--
-- Name: idx_item_movements_destination_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_destination_branch ON public.item_movements USING btree (destination_branch_id);


--
-- Name: idx_item_movements_destination_pos; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_destination_pos ON public.item_movements USING btree (destination_position_id);


--
-- Name: idx_item_movements_source_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_source_branch ON public.item_movements USING btree (source_branch_id);


--
-- Name: idx_item_movements_source_itempos; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_source_itempos ON public.item_movements USING btree (source_item_position_id);


--
-- Name: idx_item_positions_id; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE UNIQUE INDEX idx_item_positions_id ON public.item_positions USING btree (id);


--
-- Name: idx_item_positions_item; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_positions_item ON public.item_positions USING btree (item_id);


--
-- Name: idx_item_positions_position; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_positions_position ON public.item_positions USING btree (position_id);


--
-- Name: idx_item_statuses_date; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_statuses_date ON public.item_statuses USING btree (status_date);


--
-- Name: idx_item_statuses_position; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_statuses_position ON public.item_statuses USING btree (item_position_id);


--
-- Name: idx_item_statuses_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_statuses_status ON public.item_statuses USING btree (status);


--
-- Name: idx_items_dimensions; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_items_dimensions ON public.items USING btree (length, width, height);


--
-- Name: idx_mobile_app_users_active; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_active ON public.mobile_app_users USING btree (is_active);


--
-- Name: idx_mobile_app_users_employee; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_employee ON public.mobile_app_users USING btree (employee_id);


--
-- Name: idx_mobile_app_users_role; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_role ON public.mobile_app_users USING btree (role);


--
-- Name: idx_order_positions_item; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_positions_item ON public.order_positions USING btree (item_position_id);


--
-- Name: idx_order_positions_order; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_positions_order ON public.order_positions USING btree (order_id);


--
-- Name: idx_orders_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_branch ON public.orders USING btree (branch_id);


--
-- Name: idx_orders_customer; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_customer ON public.orders USING btree (customer_id);


--
-- Name: idx_orders_delivery_date; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_delivery_date ON public.orders USING btree (delivery_date);


--
-- Name: idx_orders_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_status ON public.orders USING btree (status);


--
-- Name: idx_positions_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_positions_branch ON public.positions USING btree (branch_id);


--
-- Name: idx_positions_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_positions_status ON public.positions USING btree (status);


--
-- Name: idx_positions_zone; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_positions_zone ON public.positions USING btree (zone_code);


--
-- Name: idx_raw_events_event_time; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_event_time ON public.raw_events USING btree (event_time);


--
-- Name: idx_raw_events_params; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_params ON public.raw_events USING gin (json_params);


--
-- Name: idx_raw_events_service; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_service ON public.raw_events USING btree (source_service);


--
-- Name: idx_raw_events_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_type ON public.raw_events USING btree (type);


--
-- Name: idx_statistics_assignment; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_statistics_assignment ON public.inventory_statistics USING btree (inventory_assignment_id);


--
-- Name: idx_statistics_completed; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_statistics_completed ON public.inventory_statistics USING btree (completed_at);


--
-- Name: idx_statistics_started; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_statistics_started ON public.inventory_statistics USING btree (started_at);


--
-- Name: active_assigned_tasks active_assigned_tasks_task_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks
    ADD CONSTRAINT active_assigned_tasks_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.base_tasks(task_id);


--
-- Name: active_assigned_tasks active_assigned_tasks_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks
    ADD CONSTRAINT active_assigned_tasks_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.employees(employees_id);


--
-- Name: base_tasks base_tasks_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.base_tasks
    ADD CONSTRAINT base_tasks_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- Name: check_io_employees check_io_employees_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees
    ADD CONSTRAINT check_io_employees_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- Name: check_io_employees check_io_employees_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees
    ADD CONSTRAINT check_io_employees_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employees_id);


--
-- Name: inventory_assignment_lines inventory_assignment_lines_inventory_assignment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_inventory_assignment_id_fkey FOREIGN KEY (inventory_assignment_id) REFERENCES public.inventory_assignments(id);


--
-- Name: inventory_assignment_lines inventory_assignment_lines_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- Name: inventory_assignment_lines inventory_assignment_lines_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_position_id_fkey FOREIGN KEY (position_id) REFERENCES public.positions(position_id);


--
-- Name: inventory_assignments inventory_assignments_assigned_to_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_assigned_to_user_id_fkey FOREIGN KEY (assigned_to_user_id) REFERENCES public.employees(employees_id);


--
-- Name: inventory_assignments inventory_assignments_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- Name: inventory_assignments inventory_assignments_task_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.base_tasks(task_id);


--
-- Name: inventory_discrepancies inventory_discrepancies_inventory_assignment_line_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies
    ADD CONSTRAINT inventory_discrepancies_inventory_assignment_line_id_fkey FOREIGN KEY (inventory_assignment_line_id) REFERENCES public.inventory_assignment_lines(id);


--
-- Name: inventory_discrepancies inventory_discrepancies_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies
    ADD CONSTRAINT inventory_discrepancies_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- Name: inventory_statistics inventory_statistics_inventory_assignment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics
    ADD CONSTRAINT inventory_statistics_inventory_assignment_id_fkey FOREIGN KEY (inventory_assignment_id) REFERENCES public.inventory_assignments(id);


--
-- Name: item_movements item_movements_destination_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_destination_branch_id_fkey FOREIGN KEY (destination_branch_id) REFERENCES public.branches(branch_id);


--
-- Name: item_movements item_movements_destination_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_destination_position_id_fkey FOREIGN KEY (destination_position_id) REFERENCES public.positions(position_id);


--
-- Name: item_movements item_movements_source_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_source_branch_id_fkey FOREIGN KEY (source_branch_id) REFERENCES public.branches(branch_id);


--
-- Name: item_movements item_movements_source_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_source_item_position_id_fkey FOREIGN KEY (source_item_position_id) REFERENCES public.item_positions(id);


--
-- Name: item_positions item_positions_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions
    ADD CONSTRAINT item_positions_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(item_id);


--
-- Name: item_positions item_positions_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions
    ADD CONSTRAINT item_positions_position_id_fkey FOREIGN KEY (position_id) REFERENCES public.positions(position_id);


--
-- Name: item_statuses item_statuses_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_statuses
    ADD CONSTRAINT item_statuses_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- Name: mobile_app_users mobile_app_users_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users
    ADD CONSTRAINT mobile_app_users_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employees_id);


--
-- Name: order_positions order_positions_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions
    ADD CONSTRAINT order_positions_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.positions(position_id);


--
-- Name: order_positions order_positions_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions
    ADD CONSTRAINT order_positions_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(order_id);


--
-- Name: orders orders_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- Name: positions positions_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT positions_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- PostgreSQL database dump complete
--

\unrestrict jn0PGtGNWLbEMY40x1rjTU7mSsIoPKCju1hSc14Y1xEfAckiaNi4lBlREDX1jJ4

