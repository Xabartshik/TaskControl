--
-- PostgreSQL database dump
--

\restrict DaYvMIIew1IaEpwalCmn8DSAYX1lVObz8FphB1vvFGqzPljqmzlo8Eeg1BAQ5fl

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
    started_at timestamp without time zone,
    completed_at timestamp without time zone
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
    updated_at timestamp without time zone
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
-- Data for Name: active_assigned_tasks; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.active_assigned_tasks (id, task_id, user_id, assigned_at, started_at, completed_at) FROM stdin;
\.


--
-- Data for Name: base_tasks; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.base_tasks (task_id, title, description, branch_id, type, created_at, completed_at, status, priority) FROM stdin;
\.


--
-- Data for Name: branches; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.branches (branch_id, branch_name, branch_type, address, created_at) FROM stdin;
1	Центральный склад	Warehouse	г. Москва, ул. Ленина, 1	2025-12-24 12:05:53.092085
2	Филиал Восток	Retail	г. Владивосток, ул. Портовая, 42	2025-12-24 12:05:53.092085
3	Филиал Запад	Distribution	г. Калининград, пр. Мира, 15	2025-12-24 12:05:53.092085
\.


--
-- Data for Name: check_io_employees; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) FROM stdin;
1	1	1	in	2025-07-16 08:00:00
2	1	1	out	2025-07-16 17:30:00
3	2	2	in	2025-07-16 09:15:00
4	2	2	out	2025-07-16 18:30:00
5	3	1	in	2025-07-16 07:45:00
6	3	1	out	2025-07-16 16:20:00
7	1	1	in	2025-07-17 08:05:00
8	1	1	out	2025-07-17 17:40:00
9	2	2	in	2025-07-17 09:20:00
10	2	2	out	2025-07-17 18:15:00
11	3	1	in	2025-07-17 07:50:00
12	3	1	out	2025-07-17 16:35:00
13	1	1	in	2025-07-18 08:10:00
14	1	1	in	2025-12-24 10:05:53.167316
15	2	1	in	2025-12-24 11:05:53.167316
16	3	1	in	2025-12-24 11:35:53.167316
17	1	1	in	2025-12-25 00:33:29.496874
18	2	1	in	2025-12-25 01:33:29.496874
19	3	1	in	2025-12-25 02:03:29.496874
20	1	1	in	2026-02-03 12:21:09.668395
21	2	1	in	2026-02-03 12:21:09.668395
22	1	1	in	2026-02-26 10:26:05.368204
23	2	1	in	2026-02-26 11:26:05.368204
24	3	1	in	2026-02-26 11:56:05.368204
25	1	1	in	2026-02-27 06:01:21.438548
26	2	1	in	2026-02-27 07:01:21.438548
27	3	1	in	2026-02-27 07:31:21.438548
28	1	1	in	2026-02-27 06:08:30.769305
29	2	1	in	2026-02-27 07:08:30.769305
30	3	1	in	2026-02-27 07:38:30.769305
\.


--
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.employees (employees_id, surname, name, middle_name, role, created_at) FROM stdin;
1	Иванов	Иван	Иванович	Кладовщик	2025-12-24 12:05:53.09317
2	Петрова	Мария	Сергеевна	Логист	2025-12-24 12:05:53.09317
3	Сидоров	Алексей	\N	Грузчик	2025-12-24 12:05:53.09317
\.


--
-- Data for Name: inventory_assignment_lines; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.inventory_assignment_lines (id, inventory_assignment_id, item_position_id, position_id, expected_quantity, actual_quantity, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage) FROM stdin;
\.


--
-- Data for Name: inventory_assignments; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.inventory_assignments (id, task_id, assigned_to_user_id, branch_id, status, assigned_at, completed_at) FROM stdin;
\.


--
-- Data for Name: inventory_discrepancies; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.inventory_discrepancies (id, inventory_assignment_line_id, item_position_id, expected_quantity, actual_quantity, type, note, identified_at, resolution_status, resolved_at) FROM stdin;
\.


--
-- Data for Name: inventory_statistics; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.inventory_statistics (id, inventory_assignment_id, total_positions, counted_positions, discrepancy_count, surplus_count, shortage_count, total_surplus_quantity, total_shortage_quantity, started_at, completed_at) FROM stdin;
\.


--
-- Data for Name: item_movements; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.item_movements (id, source_item_position_id, destination_position_id, source_branch_id, destination_branch_id, quantity, created_at) FROM stdin;
1	1	2	1	1	5	2025-12-24 12:05:53.106131
2	3	1	2	1	2	2025-12-24 12:05:53.106131
3	2	1	1	1	3	2025-12-24 12:05:53.112837
4	4	3	1	1	2	2025-12-24 12:05:53.112837
5	1	2	1	1	4	2025-12-24 12:05:53.112837
6	3	1	1	1	1	2025-12-24 12:05:53.112837
7	5	3	1	1	2	2025-12-24 12:05:53.112837
8	6	2	1	1	3	2025-12-24 12:05:53.112837
9	7	1	1	1	2	2025-12-24 12:05:53.112837
10	8	3	1	1	4	2025-12-24 12:05:53.112837
\.


--
-- Data for Name: item_positions; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.item_positions (id, item_id, position_id, quantity, created_at) FROM stdin;
1	1	1	10	2025-12-24 12:05:53.09665
2	2	2	5	2025-12-24 12:05:53.09665
3	3	3	2	2025-12-24 12:05:53.09665
4	1	2	8	2025-12-24 12:05:53.111944
5	2	3	6	2025-12-24 12:05:53.111944
6	3	1	3	2025-12-24 12:05:53.111944
7	1	3	4	2025-12-24 12:05:53.111944
8	2	1	7	2025-12-24 12:05:53.111944
9	3	2	2	2025-12-24 12:05:53.111944
10	1	1	5	2025-12-24 12:05:53.111944
11	2	2	9	2025-12-24 12:05:53.111944
\.


--
-- Data for Name: item_statuses; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.item_statuses (id, item_position_id, status, status_date, quantity) FROM stdin;
1	1	Reserved	2025-12-24 12:05:53.114077	3
2	2	Available	2025-12-24 12:05:53.114077	5
3	3	Shipped	2025-12-24 12:05:53.114077	2
4	4	Reserved	2025-12-24 12:05:53.114077	2
5	5	Available	2025-12-24 12:05:53.114077	7
6	6	Shipped	2025-12-24 12:05:53.114077	1
7	7	Available	2025-12-24 12:05:53.114077	5
8	8	Reserved	2025-12-24 12:05:53.114077	3
9	9	Defective	2025-12-24 12:05:53.114077	1
10	10	Available	2025-12-24 12:05:53.114077	4
\.


--
-- Data for Name: items; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.items (item_id, name, weight, length, width, height, created_at) FROM stdin;
1	Телефон No Kia	1.5	20	15	10	2025-12-24 12:05:53.094262
2	Плашка ОЗУ	0.8	10	10	5	2025-12-24 12:05:53.094262
3	Видеокарта ХХХ6090	5	50	30	20	2025-12-24 12:05:53.094262
\.


--
-- Data for Name: mobile_app_users; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.mobile_app_users (id, employee_id, password_hash, role, is_active, created_at, updated_at) FROM stdin;
1	1	a4ayc/80/OGda4BO/1o/V0etpOqiLx1JwB5S3beHW0s=	Worker	t	2026-01-14 07:27:29.195146	\N
2	2	1HNeOiZeFu7gP1lxi5tdAwGcB9i2xR+Q2jpmbuwTqzU=	Worker	t	2026-02-03 12:15:28.801992	\N
\.


--
-- Data for Name: order_positions; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.order_positions (unique_id, order_id, item_position_id, quantity, created_at) FROM stdin;
1	1	1	2	2025-12-24 12:05:53.099282
2	1	2	1	2025-12-24 12:05:53.099282
3	2	3	3	2025-12-24 12:05:53.099282
4	4	1	3	2025-12-24 12:05:53.111064
5	4	3	1	2025-12-24 12:05:53.111064
6	5	2	4	2025-12-24 12:05:53.111064
7	6	1	2	2025-12-24 12:05:53.111064
8	6	2	2	2025-12-24 12:05:53.111064
9	7	3	1	2025-12-24 12:05:53.111064
10	8	1	5	2025-12-24 12:05:53.111064
11	9	2	3	2025-12-24 12:05:53.111064
12	10	3	2	2025-12-24 12:05:53.111064
13	11	1	4	2025-12-24 12:05:53.111064
\.


--
-- Data for Name: orders; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.orders (order_id, customer_id, branch_id, delivery_date, type, status, created_at) FROM stdin;
1	1001	1	2025-07-20 14:00:00	Online	Processing	2025-12-24 12:05:53.098043
2	1002	2	2025-07-22 10:00:00	Offline	New	2025-12-24 12:05:53.098043
3	1003	1	2025-07-25 16:00:00	Wholesale	Shipped	2025-12-24 12:05:53.098043
4	1004	3	2025-07-23 12:00:00	Online	Processing	2025-12-24 12:05:53.11018
5	1005	1	2025-07-24 15:30:00	Wholesale	New	2025-12-24 12:05:53.11018
6	1006	2	2025-07-25 11:00:00	Offline	Shipped	2025-12-24 12:05:53.11018
7	1007	1	2025-07-26 14:45:00	Online	Delivered	2025-12-24 12:05:53.11018
8	1008	3	2025-07-27 10:15:00	Online	Cancelled	2025-12-24 12:05:53.11018
9	1009	2	2025-07-28 16:20:00	Wholesale	Processing	2025-12-24 12:05:53.11018
10	1010	1	2025-07-29 09:30:00	Offline	New	2025-12-24 12:05:53.11018
11	1011	3	2025-07-30 13:00:00	Online	Processing	2025-12-24 12:05:53.11018
\.


--
-- Data for Name: positions; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) FROM stdin;
1	1	Active	A	Стеллаж	A-01	Полка 2	Ячейка 3	100	50	40	2025-12-24 12:05:53.095265
2	1	Active	B	Паллет	B-05	\N	\N	120	80	100	2025-12-24 12:05:53.095265
3	2	Active	C	Контейнер	C-12	Секция 1	\N	60	40	30	2025-12-24 12:05:53.095265
\.


--
-- Data for Name: raw_events; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

COPY public.raw_events (report_id, type, json_params, event_time, source_service, created_at) FROM stdin;
1	scan	{"barcode": "123456", "location": "A-01"}	2025-07-16 10:30:00	ScannerService	2025-12-24 12:05:53.101876
2	login	{"device": "tablet", "user_id": 2}	2025-07-16 09:00:00	AuthService	2025-12-24 12:05:53.101876
3	scan	{"barcode": "789012", "location": "B-05"}	2025-07-16 11:15:00	ScannerService	2025-12-24 12:05:53.108618
4	logout	{"device": "handheld", "user_id": 1}	2025-07-16 17:35:00	AuthService	2025-12-24 12:05:53.108618
5	error	{"code": "E404", "message": "Не найдено"}	2025-07-17 10:20:00	InventoryService	2025-12-24 12:05:53.108618
6	update	{"position": "A-01", "quantity": 15}	2025-07-17 14:00:00	WMS	2025-12-24 12:05:53.108618
7	scan	{"barcode": "345678", "location": "C-12"}	2025-07-18 09:45:00	MobileApp	2025-12-24 12:05:53.108618
8	login	{"device": "desktop", "user_id": 3}	2025-07-18 08:30:00	AuthService	2025-12-24 12:05:53.108618
9	movement	{"to": "B-05", "from": "A-01", "items": 5}	2025-07-18 11:20:00	WMS	2025-12-24 12:05:53.108618
10	alert	{"type": "low_stock", "position": "C-12"}	2025-07-18 15:40:00	Monitoring	2025-12-24 12:05:53.108618
\.


--
-- Name: active_assigned_tasks_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.active_assigned_tasks_id_seq', 1, false);


--
-- Name: base_tasks_task_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.base_tasks_task_id_seq', 1, false);


--
-- Name: branches_branch_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.branches_branch_id_seq', 3, true);


--
-- Name: check_io_employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.check_io_employees_id_seq', 30, true);


--
-- Name: employees_employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.employees_employees_id_seq', 3, true);


--
-- Name: inventory_assignment_lines_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_assignment_lines_id_seq', 1, false);


--
-- Name: inventory_assignments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_assignments_id_seq', 1, false);


--
-- Name: inventory_discrepancies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_discrepancies_id_seq', 1, false);


--
-- Name: inventory_statistics_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_statistics_id_seq', 1, false);


--
-- Name: item_movements_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.item_movements_id_seq', 10, true);


--
-- Name: item_positions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.item_positions_id_seq', 11, true);


--
-- Name: item_statuses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.item_statuses_id_seq', 10, true);


--
-- Name: items_item_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.items_item_id_seq', 3, true);


--
-- Name: mobile_app_users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.mobile_app_users_id_seq', 2, true);


--
-- Name: order_positions_unique_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.order_positions_unique_id_seq', 13, true);


--
-- Name: orders_order_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.orders_order_id_seq', 11, true);


--
-- Name: positions_position_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.positions_position_id_seq', 3, true);


--
-- Name: raw_events_report_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.raw_events_report_id_seq', 10, true);


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

\unrestrict DaYvMIIew1IaEpwalCmn8DSAYX1lVObz8FphB1vvFGqzPljqmzlo8Eeg1BAQ5fl

