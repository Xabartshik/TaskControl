--
-- PostgreSQL database dump
--

\restrict dotCF4mkaHZLbltkFqmiUuAyDTJMSFZEI2zcfZkoy599w8jKapWbtrKiFsk85bO

-- Dumped from database version 15.17 (Debian 15.17-1.pgdg13+1)
-- Dumped by pg_dump version 15.17

-- Started on 2026-04-27 12:28:32 UTC

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

--
-- TOC entry 6 (class 2615 OID 16863)
-- Name: hangfire; Type: SCHEMA; Schema: -; Owner: taskservice_user
--

CREATE SCHEMA hangfire;


ALTER SCHEMA hangfire OWNER TO taskservice_user;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 277 (class 1259 OID 17155)
-- Name: aggregatedcounter; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.aggregatedcounter (
    id bigint NOT NULL,
    key text NOT NULL,
    value bigint NOT NULL,
    expireat timestamp with time zone
);


ALTER TABLE hangfire.aggregatedcounter OWNER TO taskservice_user;

--
-- TOC entry 276 (class 1259 OID 17154)
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.aggregatedcounter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.aggregatedcounter_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4012 (class 0 OID 0)
-- Dependencies: 276
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.aggregatedcounter_id_seq OWNED BY hangfire.aggregatedcounter.id;


--
-- TOC entry 259 (class 1259 OID 16870)
-- Name: counter; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.counter (
    id bigint NOT NULL,
    key text NOT NULL,
    value bigint NOT NULL,
    expireat timestamp with time zone
);


ALTER TABLE hangfire.counter OWNER TO taskservice_user;

--
-- TOC entry 258 (class 1259 OID 16869)
-- Name: counter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.counter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.counter_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4013 (class 0 OID 0)
-- Dependencies: 258
-- Name: counter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.counter_id_seq OWNED BY hangfire.counter.id;


--
-- TOC entry 261 (class 1259 OID 16878)
-- Name: hash; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.hash (
    id bigint NOT NULL,
    key text NOT NULL,
    field text NOT NULL,
    value text,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.hash OWNER TO taskservice_user;

--
-- TOC entry 260 (class 1259 OID 16877)
-- Name: hash_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.hash_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.hash_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4014 (class 0 OID 0)
-- Dependencies: 260
-- Name: hash_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.hash_id_seq OWNED BY hangfire.hash.id;


--
-- TOC entry 263 (class 1259 OID 16889)
-- Name: job; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.job (
    id bigint NOT NULL,
    stateid bigint,
    statename text,
    invocationdata jsonb NOT NULL,
    arguments jsonb NOT NULL,
    createdat timestamp with time zone NOT NULL,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.job OWNER TO taskservice_user;

--
-- TOC entry 262 (class 1259 OID 16888)
-- Name: job_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.job_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.job_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4015 (class 0 OID 0)
-- Dependencies: 262
-- Name: job_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.job_id_seq OWNED BY hangfire.job.id;


--
-- TOC entry 274 (class 1259 OID 16949)
-- Name: jobparameter; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.jobparameter (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    name text NOT NULL,
    value text,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.jobparameter OWNER TO taskservice_user;

--
-- TOC entry 273 (class 1259 OID 16948)
-- Name: jobparameter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.jobparameter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.jobparameter_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4016 (class 0 OID 0)
-- Dependencies: 273
-- Name: jobparameter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.jobparameter_id_seq OWNED BY hangfire.jobparameter.id;


--
-- TOC entry 267 (class 1259 OID 16914)
-- Name: jobqueue; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.jobqueue (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    queue text NOT NULL,
    fetchedat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.jobqueue OWNER TO taskservice_user;

--
-- TOC entry 266 (class 1259 OID 16913)
-- Name: jobqueue_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.jobqueue_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.jobqueue_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4017 (class 0 OID 0)
-- Dependencies: 266
-- Name: jobqueue_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.jobqueue_id_seq OWNED BY hangfire.jobqueue.id;


--
-- TOC entry 269 (class 1259 OID 16922)
-- Name: list; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.list (
    id bigint NOT NULL,
    key text NOT NULL,
    value text,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.list OWNER TO taskservice_user;

--
-- TOC entry 268 (class 1259 OID 16921)
-- Name: list_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.list_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.list_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4018 (class 0 OID 0)
-- Dependencies: 268
-- Name: list_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.list_id_seq OWNED BY hangfire.list.id;


--
-- TOC entry 275 (class 1259 OID 16963)
-- Name: lock; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.lock (
    resource text NOT NULL,
    updatecount integer DEFAULT 0 NOT NULL,
    acquired timestamp with time zone
);


ALTER TABLE hangfire.lock OWNER TO taskservice_user;

--
-- TOC entry 257 (class 1259 OID 16864)
-- Name: schema; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.schema (
    version integer NOT NULL
);


ALTER TABLE hangfire.schema OWNER TO taskservice_user;

--
-- TOC entry 270 (class 1259 OID 16930)
-- Name: server; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.server (
    id text NOT NULL,
    data jsonb,
    lastheartbeat timestamp with time zone NOT NULL,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.server OWNER TO taskservice_user;

--
-- TOC entry 272 (class 1259 OID 16938)
-- Name: set; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.set (
    id bigint NOT NULL,
    key text NOT NULL,
    score double precision NOT NULL,
    value text NOT NULL,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.set OWNER TO taskservice_user;

--
-- TOC entry 271 (class 1259 OID 16937)
-- Name: set_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.set_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.set_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4019 (class 0 OID 0)
-- Dependencies: 271
-- Name: set_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.set_id_seq OWNED BY hangfire.set.id;


--
-- TOC entry 265 (class 1259 OID 16899)
-- Name: state; Type: TABLE; Schema: hangfire; Owner: taskservice_user
--

CREATE TABLE hangfire.state (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    name text NOT NULL,
    reason text,
    createdat timestamp with time zone NOT NULL,
    data jsonb,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.state OWNER TO taskservice_user;

--
-- TOC entry 264 (class 1259 OID 16898)
-- Name: state_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: taskservice_user
--

CREATE SEQUENCE hangfire.state_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE hangfire.state_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4020 (class 0 OID 0)
-- Dependencies: 264
-- Name: state_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: taskservice_user
--

ALTER SEQUENCE hangfire.state_id_seq OWNED BY hangfire.state.id;


--
-- TOC entry 228 (class 1259 OID 16483)
-- Name: active_assigned_tasks; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.active_assigned_tasks (
    id integer NOT NULL,
    task_id integer NOT NULL,
    user_id integer NOT NULL,
    assigned_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    started_at timestamp without time zone,
    completed_at timestamp without time zone,
    CONSTRAINT chk_aat_completed_after_started CHECK (((completed_at IS NULL) OR ((started_at IS NOT NULL) AND (completed_at >= started_at)))),
    CONSTRAINT chk_aat_started_after_assigned CHECK (((started_at IS NULL) OR (started_at >= assigned_at)))
);


ALTER TABLE public.active_assigned_tasks OWNER TO taskservice_user;

--
-- TOC entry 227 (class 1259 OID 16482)
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
-- TOC entry 4021 (class 0 OID 0)
-- Dependencies: 227
-- Name: active_assigned_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.active_assigned_tasks_id_seq OWNED BY public.active_assigned_tasks.id;


--
-- TOC entry 226 (class 1259 OID 16458)
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
    deadline timestamp without time zone,
    priority_level integer DEFAULT 1 NOT NULL,
    source_type character varying(50),
    CONSTRAINT base_tasks_status_check CHECK (((status)::text = ANY ((ARRAY['New'::character varying, 'Assigned'::character varying, 'InProgress'::character varying, 'Completed'::character varying, 'Cancelled'::character varying, 'OnHold'::character varying, 'Blocked'::character varying])::text[])))
);


ALTER TABLE public.base_tasks OWNER TO taskservice_user;

--
-- TOC entry 225 (class 1259 OID 16457)
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
-- TOC entry 4022 (class 0 OID 0)
-- Dependencies: 225
-- Name: base_tasks_task_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.base_tasks_task_id_seq OWNED BY public.base_tasks.task_id;


--
-- TOC entry 218 (class 1259 OID 16396)
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
-- TOC entry 217 (class 1259 OID 16395)
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
-- TOC entry 4023 (class 0 OID 0)
-- Dependencies: 217
-- Name: branches_branch_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.branches_branch_id_seq OWNED BY public.branches.branch_id;


--
-- TOC entry 222 (class 1259 OID 16421)
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
-- TOC entry 221 (class 1259 OID 16420)
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
-- TOC entry 4024 (class 0 OID 0)
-- Dependencies: 221
-- Name: check_io_employees_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.check_io_employees_id_seq OWNED BY public.check_io_employees.id;


--
-- TOC entry 284 (class 1259 OID 52089)
-- Name: courier_capabilities; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.courier_capabilities (
    employee_id integer NOT NULL,
    vehicle_type_id integer NOT NULL,
    max_weight_grams double precision NOT NULL,
    max_length_mm double precision NOT NULL,
    max_width_mm double precision NOT NULL,
    max_height_mm double precision NOT NULL,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT courier_capabilities_max_height_mm_check CHECK ((max_height_mm >= (0)::double precision)),
    CONSTRAINT courier_capabilities_max_length_mm_check CHECK ((max_length_mm >= (0)::double precision)),
    CONSTRAINT courier_capabilities_max_weight_grams_check CHECK ((max_weight_grams >= (0)::double precision)),
    CONSTRAINT courier_capabilities_max_width_mm_check CHECK ((max_width_mm >= (0)::double precision))
);


ALTER TABLE public.courier_capabilities OWNER TO taskservice_user;

--
-- TOC entry 4025 (class 0 OID 0)
-- Dependencies: 284
-- Name: TABLE courier_capabilities; Type: COMMENT; Schema: public; Owner: taskservice_user
--

COMMENT ON TABLE public.courier_capabilities IS 'Параметры грузоподъемности и габаритов курьеров';


--
-- TOC entry 4026 (class 0 OID 0)
-- Dependencies: 284
-- Name: COLUMN courier_capabilities.max_weight_grams; Type: COMMENT; Schema: public; Owner: taskservice_user
--

COMMENT ON COLUMN public.courier_capabilities.max_weight_grams IS 'Максимальный вес в граммах';


--
-- TOC entry 4027 (class 0 OID 0)
-- Dependencies: 284
-- Name: COLUMN courier_capabilities.max_length_mm; Type: COMMENT; Schema: public; Owner: taskservice_user
--

COMMENT ON COLUMN public.courier_capabilities.max_length_mm IS 'Максимальная длина в мм';


--
-- TOC entry 286 (class 1259 OID 52118)
-- Name: customers; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.customers (
    customer_id integer NOT NULL,
    first_name character varying(100) NOT NULL,
    last_name character varying(100) NOT NULL,
    phone character varying(20) NOT NULL,
    email character varying(255),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.customers OWNER TO taskservice_user;

--
-- TOC entry 285 (class 1259 OID 52117)
-- Name: customers_customer_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.customers_customer_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.customers_customer_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4028 (class 0 OID 0)
-- Dependencies: 285
-- Name: customers_customer_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.customers_customer_id_seq OWNED BY public.customers.customer_id;


--
-- TOC entry 216 (class 1259 OID 16386)
-- Name: employees; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.employees (
    employees_id integer NOT NULL,
    surname character varying(100) NOT NULL,
    name character varying(100) NOT NULL,
    middle_name character varying(100),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    role_id integer DEFAULT 1 NOT NULL
);


ALTER TABLE public.employees OWNER TO taskservice_user;

--
-- TOC entry 215 (class 1259 OID 16385)
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
-- TOC entry 4029 (class 0 OID 0)
-- Dependencies: 215
-- Name: employees_employees_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.employees_employees_id_seq OWNED BY public.employees.employees_id;


--
-- TOC entry 246 (class 1259 OID 16687)
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
-- TOC entry 245 (class 1259 OID 16686)
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
-- TOC entry 4030 (class 0 OID 0)
-- Dependencies: 245
-- Name: inventory_assignment_lines_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_assignment_lines_id_seq OWNED BY public.inventory_assignment_lines.id;


--
-- TOC entry 244 (class 1259 OID 16659)
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
    started_at timestamp without time zone,
    CONSTRAINT inventory_assignments_status_check CHECK ((status = ANY (ARRAY[0, 1, 2, 3])))
);


ALTER TABLE public.inventory_assignments OWNER TO taskservice_user;

--
-- TOC entry 243 (class 1259 OID 16658)
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
-- TOC entry 4031 (class 0 OID 0)
-- Dependencies: 243
-- Name: inventory_assignments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_assignments_id_seq OWNED BY public.inventory_assignments.id;


--
-- TOC entry 248 (class 1259 OID 16714)
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
    resolved_at timestamp without time zone,
    resolution_status integer NOT NULL,
    CONSTRAINT inventory_discrepancies_actual_quantity_check CHECK ((actual_quantity >= 0)),
    CONSTRAINT inventory_discrepancies_expected_quantity_check CHECK ((expected_quantity >= 0)),
    CONSTRAINT inventory_discrepancies_resolution_status_check CHECK ((resolution_status = ANY (ARRAY[0, 1, 2, 3]))),
    CONSTRAINT inventory_discrepancies_type_check CHECK ((type = ANY (ARRAY[0, 1, 2]))),
    CONSTRAINT positive_variance CHECK (((actual_quantity - expected_quantity) IS NOT NULL))
);


ALTER TABLE public.inventory_discrepancies OWNER TO taskservice_user;

--
-- TOC entry 247 (class 1259 OID 16713)
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
-- TOC entry 4032 (class 0 OID 0)
-- Dependencies: 247
-- Name: inventory_discrepancies_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_discrepancies_id_seq OWNED BY public.inventory_discrepancies.id;


--
-- TOC entry 250 (class 1259 OID 16744)
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
-- TOC entry 249 (class 1259 OID 16743)
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
-- TOC entry 4033 (class 0 OID 0)
-- Dependencies: 249
-- Name: inventory_statistics_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.inventory_statistics_id_seq OWNED BY public.inventory_statistics.id;


--
-- TOC entry 240 (class 1259 OID 16608)
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
-- TOC entry 239 (class 1259 OID 16607)
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
-- TOC entry 4034 (class 0 OID 0)
-- Dependencies: 239
-- Name: item_movements_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.item_movements_id_seq OWNED BY public.item_movements.id;


--
-- TOC entry 236 (class 1259 OID 16565)
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
-- TOC entry 235 (class 1259 OID 16564)
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
-- TOC entry 4035 (class 0 OID 0)
-- Dependencies: 235
-- Name: item_positions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.item_positions_id_seq OWNED BY public.item_positions.id;


--
-- TOC entry 242 (class 1259 OID 16641)
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
-- TOC entry 241 (class 1259 OID 16640)
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
-- TOC entry 4036 (class 0 OID 0)
-- Dependencies: 241
-- Name: item_statuses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.item_statuses_id_seq OWNED BY public.item_statuses.id;


--
-- TOC entry 220 (class 1259 OID 16408)
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
-- TOC entry 219 (class 1259 OID 16407)
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
-- TOC entry 4037 (class 0 OID 0)
-- Dependencies: 219
-- Name: items_item_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.items_item_id_seq OWNED BY public.items.item_id;


--
-- TOC entry 256 (class 1259 OID 16837)
-- Name: mobile_app_users; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.mobile_app_users (
    id integer NOT NULL,
    employee_id integer NOT NULL,
    password_hash text NOT NULL,
    role integer NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamp without time zone,
    branch_id integer
);


ALTER TABLE public.mobile_app_users OWNER TO taskservice_user;

--
-- TOC entry 255 (class 1259 OID 16836)
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
-- TOC entry 4038 (class 0 OID 0)
-- Dependencies: 255
-- Name: mobile_app_users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.mobile_app_users_id_seq OWNED BY public.mobile_app_users.id;


--
-- TOC entry 252 (class 1259 OID 16769)
-- Name: order_assembly_assignments; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.order_assembly_assignments (
    id integer NOT NULL,
    task_id integer NOT NULL,
    order_id integer NOT NULL,
    assigned_to_user_id integer NOT NULL,
    branch_id integer NOT NULL,
    status integer NOT NULL,
    assigned_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at timestamp without time zone,
    started_at timestamp without time zone,
    CONSTRAINT order_assembly_assignments_status_check CHECK ((status = ANY (ARRAY[0, 1, 2, 3])))
);


ALTER TABLE public.order_assembly_assignments OWNER TO taskservice_user;

--
-- TOC entry 251 (class 1259 OID 16768)
-- Name: order_assembly_assignments_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.order_assembly_assignments_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_assembly_assignments_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4039 (class 0 OID 0)
-- Dependencies: 251
-- Name: order_assembly_assignments_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.order_assembly_assignments_id_seq OWNED BY public.order_assembly_assignments.id;


--
-- TOC entry 254 (class 1259 OID 16803)
-- Name: order_assembly_lines; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.order_assembly_lines (
    id integer NOT NULL,
    order_assembly_assignment_id integer NOT NULL,
    item_position_id integer NOT NULL,
    source_position_id integer NOT NULL,
    target_position_id integer NOT NULL,
    quantity integer NOT NULL,
    status integer NOT NULL,
    picked_quantity integer DEFAULT 0 NOT NULL,
    CONSTRAINT order_assembly_lines_quantity_check CHECK ((quantity > 0)),
    CONSTRAINT order_assembly_lines_status_check CHECK ((status = ANY (ARRAY[0, 1, 2, 3])))
);


ALTER TABLE public.order_assembly_lines OWNER TO taskservice_user;

--
-- TOC entry 253 (class 1259 OID 16802)
-- Name: order_assembly_lines_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.order_assembly_lines_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_assembly_lines_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4040 (class 0 OID 0)
-- Dependencies: 253
-- Name: order_assembly_lines_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.order_assembly_lines_id_seq OWNED BY public.order_assembly_lines.id;


--
-- TOC entry 234 (class 1259 OID 16544)
-- Name: order_positions; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.order_positions (
    unique_id integer NOT NULL,
    order_id integer NOT NULL,
    item_id integer NOT NULL,
    quantity integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT order_positions_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public.order_positions OWNER TO taskservice_user;

--
-- TOC entry 233 (class 1259 OID 16543)
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
-- TOC entry 4041 (class 0 OID 0)
-- Dependencies: 233
-- Name: order_positions_unique_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.order_positions_unique_id_seq OWNED BY public.order_positions.unique_id;


--
-- TOC entry 238 (class 1259 OID 16587)
-- Name: order_reservations; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.order_reservations (
    id integer NOT NULL,
    order_position_id integer NOT NULL,
    item_position_id integer,
    quantity integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT order_reservations_quantity_check CHECK ((quantity > 0))
);


ALTER TABLE public.order_reservations OWNER TO taskservice_user;

--
-- TOC entry 237 (class 1259 OID 16586)
-- Name: order_reservations_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.order_reservations_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_reservations_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4042 (class 0 OID 0)
-- Dependencies: 237
-- Name: order_reservations_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.order_reservations_id_seq OWNED BY public.order_reservations.id;


--
-- TOC entry 230 (class 1259 OID 16505)
-- Name: orders; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.orders (
    order_id integer NOT NULL,
    customer_id integer NOT NULL,
    branch_id integer NOT NULL,
    delivery_date timestamp without time zone,
    delivery_type character varying(20) NOT NULL,
    status character varying(20) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    destination_address character varying(500),
    payment_type character varying(20) NOT NULL,
    postamat_id integer,
    postamat_cell_id integer,
    CONSTRAINT orders_delivery_type_check CHECK (((delivery_type)::text = ANY ((ARRAY['Pickup'::character varying, 'Delivery'::character varying, 'Postamat'::character varying, 'Express'::character varying])::text[]))),
    CONSTRAINT orders_payment_type_check CHECK (((payment_type)::text = ANY ((ARRAY['Prepaid'::character varying, 'Postpaid'::character varying])::text[]))),
    CONSTRAINT orders_status_check CHECK (((status)::text = ANY ((ARRAY['Created'::character varying, 'Reserved'::character varying, 'Assembly'::character varying, 'Ready'::character varying, 'InTransit'::character varying, 'Completed'::character varying, 'Canceled'::character varying])::text[])))
);


ALTER TABLE public.orders OWNER TO taskservice_user;

--
-- TOC entry 229 (class 1259 OID 16504)
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
-- TOC entry 4043 (class 0 OID 0)
-- Dependencies: 229
-- Name: orders_order_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.orders_order_id_seq OWNED BY public.orders.order_id;


--
-- TOC entry 232 (class 1259 OID 16524)
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
    CONSTRAINT positions_status_check CHECK (((status)::text = ANY ((ARRAY['Active'::character varying, 'Reserved'::character varying, 'Inactive'::character varying, 'Maintenance'::character varying])::text[]))),
    CONSTRAINT positions_width_check CHECK ((width > (0)::double precision))
);


ALTER TABLE public.positions OWNER TO taskservice_user;

--
-- TOC entry 231 (class 1259 OID 16523)
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
-- TOC entry 4044 (class 0 OID 0)
-- Dependencies: 231
-- Name: positions_position_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.positions_position_id_seq OWNED BY public.positions.position_id;


--
-- TOC entry 283 (class 1259 OID 51911)
-- Name: postamat_cells; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.postamat_cells (
    cell_id integer NOT NULL,
    postamat_id integer NOT NULL,
    cell_number character varying(20) NOT NULL,
    size_label character varying(50),
    length double precision NOT NULL,
    width double precision NOT NULL,
    height double precision NOT NULL,
    status character varying(20) DEFAULT 'Available'::character varying NOT NULL,
    CONSTRAINT postamat_cells_height_check CHECK ((height > (0)::double precision)),
    CONSTRAINT postamat_cells_length_check CHECK ((length > (0)::double precision)),
    CONSTRAINT postamat_cells_status_check CHECK (((status)::text = ANY ((ARRAY['Available'::character varying, 'Reserved'::character varying, 'Occupied'::character varying, 'Maintenance'::character varying])::text[]))),
    CONSTRAINT postamat_cells_width_check CHECK ((width > (0)::double precision))
);


ALTER TABLE public.postamat_cells OWNER TO taskservice_user;

--
-- TOC entry 282 (class 1259 OID 51910)
-- Name: postamat_cells_cell_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.postamat_cells_cell_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.postamat_cells_cell_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4045 (class 0 OID 0)
-- Dependencies: 282
-- Name: postamat_cells_cell_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.postamat_cells_cell_id_seq OWNED BY public.postamat_cells.cell_id;


--
-- TOC entry 281 (class 1259 OID 51897)
-- Name: postamats; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.postamats (
    postamat_id integer NOT NULL,
    address character varying(500) NOT NULL,
    status character varying(20) DEFAULT 'Active'::character varying NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT postamats_status_check CHECK (((status)::text = ANY ((ARRAY['Active'::character varying, 'Inactive'::character varying, 'Maintenance'::character varying])::text[])))
);


ALTER TABLE public.postamats OWNER TO taskservice_user;

--
-- TOC entry 280 (class 1259 OID 51896)
-- Name: postamats_postamat_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.postamats_postamat_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.postamats_postamat_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4046 (class 0 OID 0)
-- Dependencies: 280
-- Name: postamats_postamat_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.postamats_postamat_id_seq OWNED BY public.postamats.postamat_id;


--
-- TOC entry 224 (class 1259 OID 16444)
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
-- TOC entry 223 (class 1259 OID 16443)
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
-- TOC entry 4047 (class 0 OID 0)
-- Dependencies: 223
-- Name: raw_events_report_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.raw_events_report_id_seq OWNED BY public.raw_events.report_id;


--
-- TOC entry 279 (class 1259 OID 18969)
-- Name: worker_task_efficiency; Type: TABLE; Schema: public; Owner: taskservice_user
--

CREATE TABLE public.worker_task_efficiency (
    id integer NOT NULL,
    worker_id integer NOT NULL,
    branch_id integer NOT NULL,
    task_category character varying(50) NOT NULL,
    items_processed integer DEFAULT 0 NOT NULL,
    total_duration_seconds integer DEFAULT 0 NOT NULL,
    discrepancies_found integer DEFAULT 0 NOT NULL,
    completed_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    wait_time_seconds integer DEFAULT 0 NOT NULL,
    queue_size integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.worker_task_efficiency OWNER TO taskservice_user;

--
-- TOC entry 278 (class 1259 OID 18968)
-- Name: worker_task_efficiency_id_seq; Type: SEQUENCE; Schema: public; Owner: taskservice_user
--

CREATE SEQUENCE public.worker_task_efficiency_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.worker_task_efficiency_id_seq OWNER TO taskservice_user;

--
-- TOC entry 4048 (class 0 OID 0)
-- Dependencies: 278
-- Name: worker_task_efficiency_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: taskservice_user
--

ALTER SEQUENCE public.worker_task_efficiency_id_seq OWNED BY public.worker_task_efficiency.id;


--
-- TOC entry 3507 (class 2604 OID 17158)
-- Name: aggregatedcounter id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.aggregatedcounter ALTER COLUMN id SET DEFAULT nextval('hangfire.aggregatedcounter_id_seq'::regclass);


--
-- TOC entry 3490 (class 2604 OID 16996)
-- Name: counter id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.counter ALTER COLUMN id SET DEFAULT nextval('hangfire.counter_id_seq'::regclass);


--
-- TOC entry 3491 (class 2604 OID 17005)
-- Name: hash id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.hash ALTER COLUMN id SET DEFAULT nextval('hangfire.hash_id_seq'::regclass);


--
-- TOC entry 3493 (class 2604 OID 17015)
-- Name: job id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.job ALTER COLUMN id SET DEFAULT nextval('hangfire.job_id_seq'::regclass);


--
-- TOC entry 3504 (class 2604 OID 17065)
-- Name: jobparameter id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.jobparameter ALTER COLUMN id SET DEFAULT nextval('hangfire.jobparameter_id_seq'::regclass);


--
-- TOC entry 3497 (class 2604 OID 17088)
-- Name: jobqueue id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.jobqueue ALTER COLUMN id SET DEFAULT nextval('hangfire.jobqueue_id_seq'::regclass);


--
-- TOC entry 3499 (class 2604 OID 17108)
-- Name: list id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.list ALTER COLUMN id SET DEFAULT nextval('hangfire.list_id_seq'::regclass);


--
-- TOC entry 3502 (class 2604 OID 17117)
-- Name: set id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.set ALTER COLUMN id SET DEFAULT nextval('hangfire.set_id_seq'::regclass);


--
-- TOC entry 3495 (class 2604 OID 17042)
-- Name: state id; Type: DEFAULT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.state ALTER COLUMN id SET DEFAULT nextval('hangfire.state_id_seq'::regclass);


--
-- TOC entry 3460 (class 2604 OID 16486)
-- Name: active_assigned_tasks id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks ALTER COLUMN id SET DEFAULT nextval('public.active_assigned_tasks_id_seq'::regclass);


--
-- TOC entry 3456 (class 2604 OID 16461)
-- Name: base_tasks task_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.base_tasks ALTER COLUMN task_id SET DEFAULT nextval('public.base_tasks_task_id_seq'::regclass);


--
-- TOC entry 3448 (class 2604 OID 16399)
-- Name: branches branch_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.branches ALTER COLUMN branch_id SET DEFAULT nextval('public.branches_branch_id_seq'::regclass);


--
-- TOC entry 3452 (class 2604 OID 16424)
-- Name: check_io_employees id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees ALTER COLUMN id SET DEFAULT nextval('public.check_io_employees_id_seq'::regclass);


--
-- TOC entry 3521 (class 2604 OID 52121)
-- Name: customers customer_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.customers ALTER COLUMN customer_id SET DEFAULT nextval('public.customers_customer_id_seq'::regclass);


--
-- TOC entry 3445 (class 2604 OID 16389)
-- Name: employees employees_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.employees ALTER COLUMN employees_id SET DEFAULT nextval('public.employees_employees_id_seq'::regclass);


--
-- TOC entry 3478 (class 2604 OID 16690)
-- Name: inventory_assignment_lines id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines ALTER COLUMN id SET DEFAULT nextval('public.inventory_assignment_lines_id_seq'::regclass);


--
-- TOC entry 3476 (class 2604 OID 16662)
-- Name: inventory_assignments id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments ALTER COLUMN id SET DEFAULT nextval('public.inventory_assignments_id_seq'::regclass);


--
-- TOC entry 3479 (class 2604 OID 16717)
-- Name: inventory_discrepancies id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies ALTER COLUMN id SET DEFAULT nextval('public.inventory_discrepancies_id_seq'::regclass);


--
-- TOC entry 3481 (class 2604 OID 16747)
-- Name: inventory_statistics id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics ALTER COLUMN id SET DEFAULT nextval('public.inventory_statistics_id_seq'::regclass);


--
-- TOC entry 3472 (class 2604 OID 16611)
-- Name: item_movements id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements ALTER COLUMN id SET DEFAULT nextval('public.item_movements_id_seq'::regclass);


--
-- TOC entry 3468 (class 2604 OID 16568)
-- Name: item_positions id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions ALTER COLUMN id SET DEFAULT nextval('public.item_positions_id_seq'::regclass);


--
-- TOC entry 3474 (class 2604 OID 16644)
-- Name: item_statuses id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_statuses ALTER COLUMN id SET DEFAULT nextval('public.item_statuses_id_seq'::regclass);


--
-- TOC entry 3450 (class 2604 OID 16411)
-- Name: items item_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.items ALTER COLUMN item_id SET DEFAULT nextval('public.items_item_id_seq'::regclass);


--
-- TOC entry 3487 (class 2604 OID 16840)
-- Name: mobile_app_users id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users ALTER COLUMN id SET DEFAULT nextval('public.mobile_app_users_id_seq'::regclass);


--
-- TOC entry 3483 (class 2604 OID 16772)
-- Name: order_assembly_assignments id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_assignments ALTER COLUMN id SET DEFAULT nextval('public.order_assembly_assignments_id_seq'::regclass);


--
-- TOC entry 3485 (class 2604 OID 16806)
-- Name: order_assembly_lines id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_lines ALTER COLUMN id SET DEFAULT nextval('public.order_assembly_lines_id_seq'::regclass);


--
-- TOC entry 3466 (class 2604 OID 16547)
-- Name: order_positions unique_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions ALTER COLUMN unique_id SET DEFAULT nextval('public.order_positions_unique_id_seq'::regclass);


--
-- TOC entry 3470 (class 2604 OID 16590)
-- Name: order_reservations id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_reservations ALTER COLUMN id SET DEFAULT nextval('public.order_reservations_id_seq'::regclass);


--
-- TOC entry 3462 (class 2604 OID 16508)
-- Name: orders order_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders ALTER COLUMN order_id SET DEFAULT nextval('public.orders_order_id_seq'::regclass);


--
-- TOC entry 3464 (class 2604 OID 16527)
-- Name: positions position_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.positions ALTER COLUMN position_id SET DEFAULT nextval('public.positions_position_id_seq'::regclass);


--
-- TOC entry 3518 (class 2604 OID 51914)
-- Name: postamat_cells cell_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.postamat_cells ALTER COLUMN cell_id SET DEFAULT nextval('public.postamat_cells_cell_id_seq'::regclass);


--
-- TOC entry 3515 (class 2604 OID 51900)
-- Name: postamats postamat_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.postamats ALTER COLUMN postamat_id SET DEFAULT nextval('public.postamats_postamat_id_seq'::regclass);


--
-- TOC entry 3454 (class 2604 OID 16447)
-- Name: raw_events report_id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.raw_events ALTER COLUMN report_id SET DEFAULT nextval('public.raw_events_report_id_seq'::regclass);


--
-- TOC entry 3508 (class 2604 OID 18972)
-- Name: worker_task_efficiency id; Type: DEFAULT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.worker_task_efficiency ALTER COLUMN id SET DEFAULT nextval('public.worker_task_efficiency_id_seq'::regclass);


--
-- TOC entry 3997 (class 0 OID 17155)
-- Dependencies: 277
-- Data for Name: aggregatedcounter; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (774, 'stats:succeeded:2026-04-24', 55, '2026-05-24 13:44:09.427568+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (605, 'stats:succeeded:2026-04-20', 149, '2026-05-20 12:23:06.793685+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (560, 'stats:succeeded:2026-04-19', 65, '2026-05-19 11:56:00.870109+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (193, 'stats:succeeded:2026-04-17', 583, '2026-05-17 13:16:05.65813+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1322, 'stats:succeeded:2026-04-27-08', 37, '2026-04-28 08:59:11.683296+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1, 'stats:succeeded:2026-04-14', 85, '2026-05-14 11:30:05.324174+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (704, 'stats:succeeded:2026-04-21', 112, '2026-05-21 08:18:12.166689+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1263, 'stats:succeeded:2026-04-27-06', 47, '2026-04-28 06:59:04.787974+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1347, 'stats:succeeded:2026-04-27-09', 13, '2026-04-28 09:30:05.838548+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (3, 'stats:succeeded', 1760, NULL);
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1265, 'stats:succeeded:2026-04-27', 130, '2026-05-27 11:48:10.510218+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1221, 'stats:succeeded:2026-04-26-11', 49, '2026-04-27 11:59:06.568771+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1359, 'stats:succeeded:2026-04-27-11', 5, '2026-04-28 11:48:11.510218+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (819, 'stats:succeeded:2026-04-25', 411, '2026-05-25 23:59:10.838935+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1293, 'stats:succeeded:2026-04-27-07', 28, '2026-04-28 07:52:15.030551+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1147, 'stats:succeeded:2026-04-26', 175, '2026-05-26 12:34:11.991298+00');
INSERT INTO hangfire.aggregatedcounter (id, key, value, expireat) VALUES (1253, 'stats:succeeded:2026-04-26-12', 13, '2026-04-27 12:34:12.991298+00');


--
-- TOC entry 3979 (class 0 OID 16870)
-- Dependencies: 259
-- Data for Name: counter; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.counter (id, key, value, expireat) VALUES (5342, 'stats:succeeded:2026-04-27', 1, '2026-05-27 11:49:13.787757+00');
INSERT INTO hangfire.counter (id, key, value, expireat) VALUES (5343, 'stats:succeeded:2026-04-27-11', 1, '2026-04-28 11:49:14.787757+00');
INSERT INTO hangfire.counter (id, key, value, expireat) VALUES (5344, 'stats:succeeded', 1, NULL);


--
-- TOC entry 3981 (class 0 OID 16878)
-- Dependencies: 261
-- Data for Name: hash; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (1, 'recurring-job:order-assembly-planner', 'Queue', 'default', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (3, 'recurring-job:order-assembly-planner', 'TimeZoneId', 'UTC', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (4, 'recurring-job:order-assembly-planner', 'Job', '{"t":"TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule","m":"ExecuteAsync"}', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (5, 'recurring-job:order-assembly-planner', 'CreatedAt', '1776142078261', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (7, 'recurring-job:order-assembly-planner', 'V', '2', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (2, 'recurring-job:order-assembly-planner', 'Cron', '*/1 * * * *', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (8, 'recurring-job:order-assembly-planner', 'LastExecution', '1777290554008', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (6, 'recurring-job:order-assembly-planner', 'NextExecution', '1777290600000', NULL, 0);
INSERT INTO hangfire.hash (id, key, field, value, expireat, updatecount) VALUES (9, 'recurring-job:order-assembly-planner', 'LastJobId', '1797', NULL, 0);


--
-- TOC entry 3983 (class 0 OID 16889)
-- Dependencies: 263
-- Data for Name: job; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1797, 5383, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 11:49:14.06748+00', '2026-04-28 11:49:14.787757+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1657, 4905, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:03:06.737934+00', '2026-04-27 12:03:06.852211+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1669, 4941, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:11:07.139899+00', '2026-04-28 06:11:07.248524+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1707, 5111, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:53:04.283364+00', '2026-04-28 06:53:04.353563+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1666, 4932, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:34:12.874839+00', '2026-04-27 12:34:12.991298+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1719, 5149, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:05:05.170875+00', '2026-04-28 07:05:05.238471+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1687, 5039, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:33:02.672943+00', '2026-04-28 06:33:02.778314+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1695, 5073, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:41:03.372632+00', '2026-04-28 06:41:03.487778+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1732, 5188, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:18:06.268533+00', '2026-04-28 07:18:06.338632+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1745, 5227, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:22:13.47903+00', '2026-04-28 08:22:13.568438+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1646, 4872, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:52:05.312284+00', '2026-04-27 11:52:39.147876+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1759, 5269, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:40:10.192538+00', '2026-04-28 08:40:10.263371+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1680, 5108, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:22:06.254344+00', '2026-04-28 06:52:09.071289+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1773, 5311, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:54:11.229714+00', '2026-04-28 08:54:11.302918+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1654, 4896, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:00:06.532547+00', '2026-04-27 12:00:06.630269+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1788, 5356, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:27:05.49041+00', '2026-04-28 09:27:05.56844+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1670, 4944, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:12:07.252227+00', '2026-04-28 06:12:07.37288+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1720, 5152, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:06:05.266823+00', '2026-04-28 07:06:05.33107+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1733, 5191, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:19:06.330975+00', '2026-04-28 07:19:06.39965+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1688, 5046, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:34:02.795335+00', '2026-04-28 06:34:02.882721+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1696, 5076, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:42:03.439345+00', '2026-04-28 06:42:03.516466+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1708, 5114, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:54:04.348802+00', '2026-04-28 06:54:04.417281+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1746, 5230, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:23:13.533316+00', '2026-04-28 08:23:13.621407+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1760, 5272, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:41:10.263449+00', '2026-04-28 08:41:10.334712+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1774, 5314, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:55:11.301867+00', '2026-04-28 08:55:11.385576+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1681, 5116, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:24:49.230124+00', '2026-04-28 06:54:49.514723+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1789, 5359, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:28:05.571989+00', '2026-04-28 09:28:05.663232+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1778, 5326, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:59:11.612327+00', '2026-04-28 08:59:11.683296+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1775, 5317, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:56:11.401857+00', '2026-04-28 08:56:11.47766+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1790, 5362, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:29:05.668323+00', '2026-04-28 09:29:05.748831+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1698, 5082, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:44:03.569299+00', '2026-04-28 06:44:03.639787+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1682, 5049, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:28:49.414694+00', '2026-04-28 06:34:17.933899+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1671, 4947, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:13:07.356942+00', '2026-04-28 06:13:07.461623+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1697, 5079, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:43:03.504154+00', '2026-04-28 06:43:03.575793+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1709, 5119, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:55:04.42208+00', '2026-04-28 06:55:04.492415+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1721, 5155, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:07:05.354647+00', '2026-04-28 07:07:05.428797+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1658, 4908, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:12:51.436938+00', '2026-04-27 12:12:53.004926+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1734, 5194, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:20:06.397941+00', '2026-04-28 07:20:06.48797+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1689, 5055, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:35:02.87798+00', '2026-04-28 06:35:02.966519+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1747, 5233, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:24:13.584494+00', '2026-04-28 08:24:13.663536+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1761, 5275, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:42:10.33162+00', '2026-04-28 08:42:10.410946+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1776, 5320, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:57:11.461394+00', '2026-04-28 08:57:11.530349+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1791, 5365, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:30:05.760802+00', '2026-04-28 09:30:05.838548+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1647, 4875, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:53:09.053284+00', '2026-04-27 11:53:27.771983+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1777, 5323, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:58:11.52213+00', '2026-04-28 08:58:11.584084+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1667, 4935, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:09:51.756417+00', '2026-04-28 06:09:52.953285+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1655, 4899, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:01:06.596751+00', '2026-04-27 12:01:06.702584+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1724, 5164, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:10:05.60573+00', '2026-04-28 07:10:05.667343+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1683, 5052, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:29:04.515742+00', '2026-04-28 06:34:47.975437+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1672, 4950, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:14:07.441471+00', '2026-04-28 06:14:07.543149+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1659, 4911, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:13:06.654569+00', '2026-04-27 12:13:06.778492+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1699, 5085, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:45:03.625253+00', '2026-04-28 06:45:03.723592+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1710, 5122, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:56:04.507823+00', '2026-04-28 06:56:04.579767+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1722, 5158, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:08:05.444394+00', '2026-04-28 07:08:05.512255+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1735, 5197, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:46:06.172081+00', '2026-04-28 07:46:08.425934+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1690, 5058, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:36:02.945167+00', '2026-04-28 06:36:03.019798+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1748, 5236, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:27:44.239611+00', '2026-04-28 08:27:45.546838+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1762, 5278, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:43:10.400429+00', '2026-04-28 08:43:10.470842+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1648, 4878, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:54:12.764711+00', '2026-04-27 11:54:12.973375+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1779, 5329, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:00:11.675097+00', '2026-04-28 09:00:11.740384+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1792, 5368, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 11:44:55.924828+00', '2026-04-28 11:44:58.511427+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1668, 4938, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:10:06.989186+00', '2026-04-28 06:10:07.102203+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1656, 4902, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:02:06.652334+00', '2026-04-27 12:02:06.75855+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1793, 5371, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 11:45:11.160658+00', '2026-04-28 11:45:16.760348+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1684, 5043, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:30:04.742445+00', '2026-04-28 06:33:32.871175+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1673, 4953, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:15:07.532756+00', '2026-04-28 06:15:07.633612+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1700, 5088, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:46:03.698339+00', '2026-04-28 06:46:03.769568+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1660, 4914, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:14:06.805683+00', '2026-04-27 12:14:06.899542+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1711, 5125, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:57:04.576149+00', '2026-04-28 06:57:04.654479+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1723, 5161, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:09:05.531963+00', '2026-04-28 07:09:05.601518+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1736, 5200, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:47:06.271186+00', '2026-04-28 07:47:17.395401+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1749, 5239, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:30:09.279648+00', '2026-04-28 08:30:10.671751+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1763, 5281, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:44:10.473126+00', '2026-04-28 08:44:10.548897+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1649, 4881, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:55:12.848506+00', '2026-04-27 11:55:12.97452+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1780, 5332, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:01:11.731878+00', '2026-04-28 09:01:11.794008+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1691, 5061, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:37:03.019468+00', '2026-04-28 06:37:03.093484+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1794, 5374, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 11:46:11.229074+00', '2026-04-28 11:46:17.10114+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1661, 4917, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:15:06.944542+00', '2026-04-27 12:15:07.031622+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1674, 4956, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:16:07.620316+00', '2026-04-28 06:16:07.708668+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1692, 5064, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:38:03.108697+00', '2026-04-28 06:38:03.186062+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1701, 5091, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:47:03.767111+00', '2026-04-28 06:47:03.844808+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1712, 5128, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:58:04.659332+00', '2026-04-28 06:58:04.732872+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1725, 5167, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:11:05.680948+00', '2026-04-28 07:11:05.870888+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1650, 4884, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:56:12.917789+00', '2026-04-27 11:56:13.023862+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1737, 5203, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:48:04.983448+00', '2026-04-28 07:48:06.089714+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1750, 5242, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:31:09.448659+00', '2026-04-28 08:31:09.616966+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1677, 4965, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:19:07.927369+00', '2026-04-28 06:19:08.025635+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1764, 5284, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:45:10.561875+00', '2026-04-28 08:45:10.634589+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1781, 5335, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:08:44.287481+00', '2026-04-28 09:08:45.930514+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1795, 5377, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 11:47:11.316544+00', '2026-04-28 11:47:11.405345+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1685, 5034, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:31:04.83951+00', '2026-04-28 06:32:33.608615+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1675, 4959, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:17:07.683236+00', '2026-04-28 06:17:07.777712+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1702, 5094, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:48:03.857114+00', '2026-04-28 06:48:03.927876+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1662, 4920, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:16:07.035049+00', '2026-04-27 12:16:07.117058+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1686, 5040, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:32:04.906919+00', '2026-04-28 06:33:02.805768+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1713, 5131, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:59:04.716901+00', '2026-04-28 06:59:04.787974+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1644, 4866, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:50:05.15035+00', '2026-04-27 11:50:05.246345+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1726, 5170, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:12:05.756897+00', '2026-04-28 07:12:05.825557+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1738, 5206, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:49:05.115845+00', '2026-04-28 07:49:05.212862+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1751, 5245, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:32:09.532254+00', '2026-04-28 08:32:09.619671+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1765, 5287, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:46:10.645143+00', '2026-04-28 08:46:10.710846+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1782, 5338, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:21:49.889463+00', '2026-04-28 09:21:51.792019+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1693, 5067, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:39:03.180338+00', '2026-04-28 06:39:03.257956+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1651, 4887, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:57:21.201427+00', '2026-04-27 11:57:22.697233+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1796, 5380, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 11:48:11.431257+00', '2026-04-28 11:48:11.510218+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1676, 4962, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:18:07.812103+00', '2026-04-28 06:18:07.90156+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1663, 4923, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:17:07.107578+00', '2026-04-27 12:17:07.191852+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1694, 5070, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:40:03.28572+00', '2026-04-28 06:40:03.397082+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1703, 5097, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:49:03.954796+00', '2026-04-28 06:49:04.035686+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1714, 5134, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:00:04.777937+00', '2026-04-28 07:00:04.852623+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1727, 5173, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:13:05.828118+00', '2026-04-28 07:13:05.912502+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1739, 5209, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:50:05.211675+00', '2026-04-28 07:50:05.314582+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1652, 4890, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:58:06.35415+00', '2026-04-27 11:58:06.470376+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1752, 5248, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:33:09.612806+00', '2026-04-28 08:33:09.687706+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1766, 5290, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:47:10.701544+00', '2026-04-28 08:47:10.76944+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1783, 5341, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:22:05.021823+00', '2026-04-28 09:22:10.150337+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1678, 4968, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:20:08.013124+00', '2026-04-28 06:20:08.126294+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1704, 5100, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:50:04.024162+00', '2026-04-28 06:50:04.105379+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1715, 5137, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:01:04.835775+00', '2026-04-28 07:01:04.911654+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1728, 5176, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:14:05.906544+00', '2026-04-28 07:14:05.998472+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1740, 5212, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:51:22.600414+00', '2026-04-28 07:51:23.925455+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1753, 5251, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:34:09.692953+00', '2026-04-28 08:34:09.771062+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1767, 5293, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:48:10.782886+00', '2026-04-28 08:48:10.862188+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1784, 5344, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:23:05.15265+00', '2026-04-28 09:23:10.426559+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1729, 5179, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:15:05.986098+00', '2026-04-28 07:15:06.064515+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1786, 5350, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:25:05.317938+00', '2026-04-28 09:25:05.395361+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1679, 4971, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:21:06.076388+00', '2026-04-28 06:21:07.516645+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1705, 5103, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:51:04.101973+00', '2026-04-28 06:51:04.174543+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1716, 5140, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:02:04.910467+00', '2026-04-28 07:02:04.985058+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1730, 5182, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:16:06.076553+00', '2026-04-28 07:16:06.179055+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1643, 4863, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:49:04.699483+00', '2026-04-27 11:49:05.310372+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1664, 4926, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:32:56.871069+00', '2026-04-27 12:32:58.345993+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1741, 5215, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:52:14.555172+00', '2026-04-28 07:52:15.030551+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1754, 5254, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:35:09.76925+00', '2026-04-28 08:35:09.842327+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1768, 5296, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:49:10.852234+00', '2026-04-28 08:49:10.936024+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1785, 5347, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:24:05.238438+00', '2026-04-28 09:24:05.310123+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1653, 4893, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:59:06.43111+00', '2026-04-27 11:59:06.568771+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1706, 5106, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 06:52:04.189859+00', '2026-04-28 06:52:04.276335+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1717, 5143, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:03:04.999314+00', '2026-04-28 07:03:05.073064+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1731, 5185, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:17:06.192378+00', '2026-04-28 07:17:06.2674+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1742, 5218, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:19:28.190249+00', '2026-04-28 08:19:31.116575+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1755, 5257, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:36:09.854083+00', '2026-04-28 08:36:09.958786+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1769, 5299, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:50:10.959028+00', '2026-04-28 08:50:11.156894+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1787, 5353, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 09:26:05.403724+00', '2026-04-28 09:26:05.481768+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1718, 5146, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 07:04:05.104967+00', '2026-04-28 07:04:05.174137+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1743, 5221, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:20:13.331704+00', '2026-04-28 08:20:18.586406+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1756, 5260, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:37:09.925781+00', '2026-04-28 08:37:10.019994+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1770, 5302, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:51:11.038288+00', '2026-04-28 08:51:11.116515+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1744, 5224, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:21:13.401952+00', '2026-04-28 08:21:13.509084+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1757, 5263, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:38:09.999203+00', '2026-04-28 08:38:10.104362+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1771, 5305, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:52:11.10874+00', '2026-04-28 08:52:11.173396+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1758, 5266, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:39:10.098419+00', '2026-04-28 08:39:10.198333+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1772, 5308, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-27 08:53:11.17563+00', '2026-04-28 08:53:11.242686+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1645, 4869, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 11:51:05.231602+00', '2026-04-27 11:51:05.340911+00', 0);
INSERT INTO hangfire.job (id, stateid, statename, invocationdata, arguments, createdat, expireat, updatecount) VALUES (1665, 4929, 'Succeeded', '{"Type": "TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob, TaskControl.TaskModule", "Method": "ExecuteAsync", "Arguments": "[]", "ParameterTypes": "[]"}', '[]', '2026-04-26 12:33:11.991239+00', '2026-04-27 12:33:12.142753+00', 0);


--
-- TOC entry 3994 (class 0 OID 16949)
-- Dependencies: 274
-- Data for Name: jobparameter; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4939, 1669, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4940, 1669, 'Time', '1777270267', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4941, 1669, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4972, 1680, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4973, 1680, 'Time', '1777270926', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4974, 1680, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5001, 1688, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5002, 1688, 'Time', '1777271642', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5003, 1688, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5034, 1699, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5035, 1699, 'Time', '1777272303', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4870, 1646, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4871, 1646, 'Time', '1777204325', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4872, 1646, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4882, 1650, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4883, 1650, 'Time', '1777204572', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4884, 1650, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4903, 1657, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4904, 1657, 'Time', '1777204986', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4905, 1657, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4936, 1668, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4937, 1668, 'Time', '1777270206', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4938, 1668, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5036, 1699, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5064, 1709, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5065, 1709, 'Time', '1777272904', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5066, 1709, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5094, 1719, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5095, 1719, 'Time', '1777273505', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5096, 1719, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5133, 1732, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5134, 1732, 'Time', '1777274286', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5135, 1732, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5163, 1742, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5164, 1742, 'Time', '1777277968', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5165, 1742, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5196, 1753, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5197, 1753, 'Time', '1777278849', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5198, 1753, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5226, 1763, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5227, 1763, 'Time', '1777279450', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5228, 1763, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5265, 1776, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5266, 1776, 'Time', '1777280231', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5267, 1776, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5268, 1777, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5269, 1777, 'Time', '1777280291', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5270, 1777, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5310, 1791, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5311, 1791, 'Time', '1777282205', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5312, 1791, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4942, 1670, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4943, 1670, 'Time', '1777270327', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4944, 1670, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4978, 1682, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4979, 1682, 'Time', '1777271329', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4980, 1682, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5004, 1689, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5005, 1689, 'Time', '1777271702', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5006, 1689, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4873, 1647, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4874, 1647, 'Time', '1777204389', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4875, 1647, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4906, 1658, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4907, 1658, 'Time', '1777205571', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4908, 1658, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5037, 1700, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5038, 1700, 'Time', '1777272363', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5039, 1700, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5067, 1710, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5068, 1710, 'Time', '1777272964', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5069, 1710, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5097, 1720, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5098, 1720, 'Time', '1777273565', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5099, 1720, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5136, 1733, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5137, 1733, 'Time', '1777274346', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5138, 1733, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5166, 1743, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5167, 1743, 'Time', '1777278013', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5168, 1743, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5199, 1754, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5200, 1754, 'Time', '1777278909', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5201, 1754, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5229, 1764, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5230, 1764, 'Time', '1777279510', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5231, 1764, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5274, 1779, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5275, 1779, 'Time', '1777280411', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5276, 1779, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5313, 1792, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5314, 1792, 'Time', '1777290295', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5315, 1792, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5316, 1793, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5317, 1793, 'Time', '1777290311', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5318, 1793, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4945, 1671, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4946, 1671, 'Time', '1777270387', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4947, 1671, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4981, 1682, 'RetryCount', '4', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5007, 1690, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5008, 1690, 'Time', '1777271762', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5009, 1690, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5040, 1701, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5041, 1701, 'Time', '1777272423', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4876, 1648, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4877, 1648, 'Time', '1777204452', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4878, 1648, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4909, 1659, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4910, 1659, 'Time', '1777205586', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4911, 1659, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5042, 1701, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5070, 1711, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5071, 1711, 'Time', '1777273024', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5072, 1711, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5103, 1722, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5104, 1722, 'Time', '1777273685', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5105, 1722, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5109, 1724, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5110, 1724, 'Time', '1777273805', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5111, 1724, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5139, 1734, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5140, 1734, 'Time', '1777274406', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5141, 1734, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5169, 1744, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5170, 1744, 'Time', '1777278073', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5171, 1744, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5178, 1747, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5179, 1747, 'Time', '1777278253', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5180, 1747, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5202, 1755, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5203, 1755, 'Time', '1777278969', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5204, 1755, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5232, 1765, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5233, 1765, 'Time', '1777279570', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5234, 1765, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5277, 1780, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5278, 1780, 'Time', '1777280471', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5279, 1780, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5319, 1794, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5320, 1794, 'Time', '1777290371', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5321, 1794, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4948, 1672, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4949, 1672, 'Time', '1777270447', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4950, 1672, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4982, 1683, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4983, 1683, 'Time', '1777271344', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4984, 1683, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4997, 1686, 'RetryCount', '1', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5010, 1691, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5011, 1691, 'Time', '1777271823', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5012, 1691, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5043, 1702, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5044, 1702, 'Time', '1777272483', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5045, 1702, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5073, 1712, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5074, 1712, 'Time', '1777273084', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4879, 1649, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4880, 1649, 'Time', '1777204512', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4881, 1649, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4912, 1660, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4913, 1660, 'Time', '1777205646', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4914, 1660, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4918, 1662, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4919, 1662, 'Time', '1777205767', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4920, 1662, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5075, 1712, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5106, 1723, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5107, 1723, 'Time', '1777273745', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5108, 1723, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5142, 1735, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5143, 1735, 'Time', '1777275966', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5144, 1735, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5172, 1745, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5173, 1745, 'Time', '1777278133', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5174, 1745, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5205, 1756, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5206, 1756, 'Time', '1777279029', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5207, 1756, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5235, 1766, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5236, 1766, 'Time', '1777279630', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5237, 1766, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5253, 1772, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5254, 1772, 'Time', '1777279991', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5255, 1772, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5280, 1781, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5281, 1781, 'Time', '1777280924', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5282, 1781, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5322, 1795, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5323, 1795, 'Time', '1777290431', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5324, 1795, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4885, 1651, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4886, 1651, 'Time', '1777204641', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4887, 1651, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4915, 1661, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4916, 1661, 'Time', '1777205706', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4917, 1661, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4951, 1673, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4952, 1673, 'Time', '1777270507', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4953, 1673, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4985, 1683, 'RetryCount', '4', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4989, 1684, 'RetryCount', '3', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5013, 1692, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5014, 1692, 'Time', '1777271883', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5015, 1692, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5046, 1703, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5047, 1703, 'Time', '1777272543', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5048, 1703, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5076, 1713, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5077, 1713, 'Time', '1777273144', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5078, 1713, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5112, 1725, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5113, 1725, 'Time', '1777273865', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5114, 1725, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5145, 1736, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5146, 1736, 'Time', '1777276026', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5147, 1736, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5175, 1746, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5176, 1746, 'Time', '1777278193', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5177, 1746, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5208, 1757, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5209, 1757, 'Time', '1777279089', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5210, 1757, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5238, 1767, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5239, 1767, 'Time', '1777279690', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5240, 1767, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5283, 1782, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5284, 1782, 'Time', '1777281709', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5285, 1782, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5325, 1796, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5326, 1796, 'Time', '1777290491', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5327, 1796, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4954, 1674, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4955, 1674, 'Time', '1777270567', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4956, 1674, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4963, 1677, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4964, 1677, 'Time', '1777270747', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4965, 1677, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4986, 1684, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4987, 1684, 'Time', '1777271404', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4988, 1684, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4888, 1652, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4889, 1652, 'Time', '1777204686', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4890, 1652, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4921, 1663, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4922, 1663, 'Time', '1777205827', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4923, 1663, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5016, 1693, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5017, 1693, 'Time', '1777271943', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5018, 1693, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5049, 1704, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5050, 1704, 'Time', '1777272604', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5051, 1704, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5079, 1714, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5080, 1714, 'Time', '1777273204', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5081, 1714, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5115, 1726, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5116, 1726, 'Time', '1777273925', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5117, 1726, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5148, 1737, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5149, 1737, 'Time', '1777276084', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5150, 1737, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5181, 1748, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5182, 1748, 'Time', '1777278464', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5183, 1748, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5211, 1758, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5212, 1758, 'Time', '1777279150', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5213, 1758, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5241, 1768, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5242, 1768, 'Time', '1777279750', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5243, 1768, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5286, 1783, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5287, 1783, 'Time', '1777281725', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5288, 1783, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5328, 1797, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5329, 1797, 'Time', '1777290554', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5330, 1797, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4891, 1653, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4892, 1653, 'Time', '1777204746', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4893, 1653, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4924, 1664, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4925, 1664, 'Time', '1777206776', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4926, 1664, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4957, 1675, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4958, 1675, 'Time', '1777270627', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4959, 1675, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4990, 1685, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4991, 1685, 'Time', '1777271464', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4992, 1685, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5019, 1694, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5020, 1694, 'Time', '1777272003', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5021, 1694, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5052, 1705, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5053, 1705, 'Time', '1777272664', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5054, 1705, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5082, 1715, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5083, 1715, 'Time', '1777273264', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5084, 1715, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5118, 1727, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5119, 1727, 'Time', '1777273985', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5120, 1727, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5151, 1738, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5152, 1738, 'Time', '1777276145', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5153, 1738, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5184, 1749, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5185, 1749, 'Time', '1777278609', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5186, 1749, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5214, 1759, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5215, 1759, 'Time', '1777279210', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5216, 1759, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5244, 1769, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5245, 1769, 'Time', '1777279810', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5246, 1769, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5289, 1784, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5290, 1784, 'Time', '1777281785', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5291, 1784, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5295, 1786, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5296, 1786, 'Time', '1777281905', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5297, 1786, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4861, 1643, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4862, 1643, 'Time', '1777204144', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4863, 1643, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4894, 1654, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4895, 1654, 'Time', '1777204806', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4896, 1654, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4927, 1665, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4928, 1665, 'Time', '1777206791', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4929, 1665, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4960, 1676, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4961, 1676, 'Time', '1777270687', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4962, 1676, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4993, 1685, 'RetryCount', '2', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5022, 1695, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5023, 1695, 'Time', '1777272063', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5024, 1695, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5055, 1706, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5056, 1706, 'Time', '1777272724', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5057, 1706, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5085, 1716, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5086, 1716, 'Time', '1777273324', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5087, 1716, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5121, 1728, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5122, 1728, 'Time', '1777274045', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5123, 1728, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5124, 1729, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5125, 1729, 'Time', '1777274105', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5126, 1729, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5154, 1739, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5155, 1739, 'Time', '1777276205', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5156, 1739, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5187, 1750, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5188, 1750, 'Time', '1777278669', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5189, 1750, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5217, 1760, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5218, 1760, 'Time', '1777279270', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5219, 1760, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5247, 1770, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5248, 1770, 'Time', '1777279871', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5249, 1770, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5250, 1771, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5251, 1771, 'Time', '1777279931', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5252, 1771, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5292, 1785, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5293, 1785, 'Time', '1777281845', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5294, 1785, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4864, 1644, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4865, 1644, 'Time', '1777204205', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4866, 1644, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4897, 1655, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4898, 1655, 'Time', '1777204866', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4899, 1655, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4930, 1666, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4931, 1666, 'Time', '1777206852', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4932, 1666, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4966, 1678, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4967, 1678, 'Time', '1777270808', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4968, 1678, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4994, 1686, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4995, 1686, 'Time', '1777271524', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4996, 1686, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5025, 1696, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5026, 1696, 'Time', '1777272123', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5027, 1696, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5058, 1707, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5059, 1707, 'Time', '1777272784', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5060, 1707, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5088, 1717, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5089, 1717, 'Time', '1777273384', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5090, 1717, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5100, 1721, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5101, 1721, 'Time', '1777273625', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5102, 1721, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5127, 1730, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5128, 1730, 'Time', '1777274166', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5129, 1730, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5157, 1740, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5158, 1740, 'Time', '1777276282', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5159, 1740, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5190, 1751, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5191, 1751, 'Time', '1777278729', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5192, 1751, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5220, 1761, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5221, 1761, 'Time', '1777279330', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5222, 1761, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5256, 1773, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5257, 1773, 'Time', '1777280051', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5258, 1773, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5298, 1787, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5299, 1787, 'Time', '1777281965', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5300, 1787, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4867, 1645, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4868, 1645, 'Time', '1777204265', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4869, 1645, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4900, 1656, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4901, 1656, 'Time', '1777204926', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4902, 1656, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4933, 1667, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4934, 1667, 'Time', '1777270191', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4935, 1667, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4969, 1679, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4970, 1679, 'Time', '1777270866', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4971, 1679, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4975, 1681, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4976, 1681, 'Time', '1777271089', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4977, 1681, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4998, 1687, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (4999, 1687, 'Time', '1777271582', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5000, 1687, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5028, 1697, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5029, 1697, 'Time', '1777272183', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5030, 1697, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5031, 1698, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5032, 1698, 'Time', '1777272243', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5033, 1698, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5061, 1708, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5062, 1708, 'Time', '1777272844', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5063, 1708, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5091, 1718, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5092, 1718, 'Time', '1777273445', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5093, 1718, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5130, 1731, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5131, 1731, 'Time', '1777274226', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5132, 1731, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5160, 1741, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5161, 1741, 'Time', '1777276334', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5162, 1741, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5193, 1752, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5194, 1752, 'Time', '1777278789', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5195, 1752, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5223, 1762, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5224, 1762, 'Time', '1777279390', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5225, 1762, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5259, 1774, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5260, 1774, 'Time', '1777280111', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5261, 1774, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5262, 1775, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5263, 1775, 'Time', '1777280171', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5264, 1775, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5271, 1778, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5272, 1778, 'Time', '1777280351', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5273, 1778, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5301, 1788, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5302, 1788, 'Time', '1777282025', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5303, 1788, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5304, 1789, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5305, 1789, 'Time', '1777282085', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5306, 1789, 'CurrentCulture', '"ru-RU"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5307, 1790, 'RecurringJobId', '"order-assembly-planner"', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5308, 1790, 'Time', '1777282145', 0);
INSERT INTO hangfire.jobparameter (id, jobid, name, value, updatecount) VALUES (5309, 1790, 'CurrentCulture', '"ru-RU"', 0);


--
-- TOC entry 3987 (class 0 OID 16914)
-- Dependencies: 267
-- Data for Name: jobqueue; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1750, 1736, 'default', '2026-04-27 11:45:58.134902+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1739, 1725, 'default', '2026-04-27 11:45:58.134727+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1809, 1795, 'default', '2026-04-27 11:47:11.329938+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1709, 1695, 'default', '2026-04-27 11:46:06.951667+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1494, 1494, 'default', '2026-04-27 11:46:06.959798+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1808, 1794, 'default', '2026-04-27 11:46:16.983723+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1524, 1524, 'default', '2026-04-27 11:46:17.000422+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1798, 1784, 'default', '2026-04-27 11:46:17.017517+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1782, 1768, 'default', '2026-04-27 11:44:55.909681+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1466, 1466, 'default', '2026-04-27 11:44:56.10066+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1467, 1467, 'default', '2026-04-27 11:44:56.106374+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1528, 1528, 'default', '2026-04-27 11:44:56.111887+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1525, 1525, 'default', '2026-04-27 11:44:56.125595+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1496, 1496, 'default', '2026-04-27 11:44:57.611866+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1659, 1659, 'default', '2026-04-27 11:46:17.037846+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1529, 1529, 'default', '2026-04-27 11:44:57.611869+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1526, 1526, 'default', '2026-04-27 11:44:58.520727+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1606, 1606, 'default', '2026-04-27 11:46:18.943236+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1635, 1635, 'default', '2026-04-27 11:46:18.989392+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1636, 1636, 'default', '2026-04-27 11:46:20.189893+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1740, 1726, 'default', '2026-04-27 11:46:29.035664+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1710, 1696, 'default', '2026-04-27 11:46:29.042372+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1669, 1669, 'default', '2026-04-27 11:46:29.054878+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1752, 1738, 'default', '2026-04-27 11:46:29.135778+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1784, 1770, 'default', '2026-04-27 11:46:29.156782+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1758, 1744, 'default', '2026-04-27 11:46:29.165461+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1799, 1785, 'default', '2026-04-27 11:46:29.178954+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1789, 1775, 'default', '2026-04-27 11:46:29.196161+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1792, 1778, 'default', '2026-04-27 11:46:29.218202+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1465, 1465, 'default', '2026-04-27 11:44:58.626033+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1495, 1495, 'default', '2026-04-27 11:44:59.650473+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1534, 1534, 'default', '2026-04-27 11:45:06.632821+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1468, 1468, 'default', '2026-04-27 11:45:06.633047+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1527, 1527, 'default', '2026-04-27 11:45:06.632995+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1443, 1443, 'default', '2026-04-27 11:45:06.633008+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1555, 1555, 'default', '2026-04-27 11:45:06.644802+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1497, 1497, 'default', '2026-04-27 11:45:06.64687+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1499, 1499, 'default', '2026-04-27 11:45:06.64757+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1498, 1498, 'default', '2026-04-27 11:45:06.647586+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1501, 1501, 'default', '2026-04-27 11:45:06.64757+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1470, 1470, 'default', '2026-04-27 11:45:07.667292+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1502, 1502, 'default', '2026-04-27 11:45:07.698572+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1500, 1500, 'default', '2026-04-27 11:45:07.698614+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1532, 1532, 'default', '2026-04-27 11:45:07.698773+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1469, 1469, 'default', '2026-04-27 11:45:08.572685+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1471, 1471, 'default', '2026-04-27 11:45:08.681434+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1473, 1473, 'default', '2026-04-27 11:45:08.681437+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1474, 1474, 'default', '2026-04-27 11:45:08.712758+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1530, 1530, 'default', '2026-04-27 11:45:09.69664+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1472, 1472, 'default', '2026-04-27 11:45:16.683665+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1444, 1444, 'default', '2026-04-27 11:45:16.683724+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1531, 1531, 'default', '2026-04-27 11:45:16.683592+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1442, 1442, 'default', '2026-04-27 11:45:16.683421+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1475, 1475, 'default', '2026-04-27 11:45:16.696519+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1507, 1507, 'default', '2026-04-27 11:45:16.697077+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1503, 1503, 'default', '2026-04-27 11:45:16.706429+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1504, 1504, 'default', '2026-04-27 11:45:16.765963+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1446, 1446, 'default', '2026-04-27 11:45:17.711451+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1535, 1535, 'default', '2026-04-27 11:45:17.758035+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1538, 1538, 'default', '2026-04-27 11:45:18.628725+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1445, 1445, 'default', '2026-04-27 11:45:18.752887+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1505, 1505, 'default', '2026-04-27 11:45:18.765591+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1536, 1536, 'default', '2026-04-27 11:45:19.740167+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1506, 1506, 'default', '2026-04-27 11:45:26.751876+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1533, 1533, 'default', '2026-04-27 11:45:26.751844+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1476, 1476, 'default', '2026-04-27 11:45:26.751935+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1447, 1447, 'default', '2026-04-27 11:45:26.751931+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1477, 1477, 'default', '2026-04-27 11:45:26.751894+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1448, 1448, 'default', '2026-04-27 11:45:26.766497+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1537, 1537, 'default', '2026-04-27 11:45:26.766574+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1810, 1796, 'default', '2026-04-27 11:48:11.441203+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1518, 1518, 'default', '2026-04-27 11:45:56.904017+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1576, 1576, 'default', '2026-04-27 11:45:56.916696+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1607, 1607, 'default', '2026-04-27 11:46:17.113223+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1637, 1637, 'default', '2026-04-27 11:46:18.243681+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1660, 1660, 'default', '2026-04-27 11:46:19.004735+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1556, 1556, 'default', '2026-04-27 11:45:16.683826+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1662, 1662, 'default', '2026-04-27 11:46:27.038268+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1783, 1769, 'default', '2026-04-27 11:45:28.817705+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1581, 1581, 'default', '2026-04-27 11:45:46.862263+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1711, 1697, 'default', '2026-04-27 11:46:29.06255+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1741, 1727, 'default', '2026-04-27 11:46:29.066215+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1670, 1670, 'default', '2026-04-27 11:46:29.069931+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1712, 1698, 'default', '2026-04-27 11:46:29.078931+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1751, 1737, 'default', '2026-04-27 11:46:29.116138+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1753, 1739, 'default', '2026-04-27 11:46:29.144398+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1785, 1771, 'default', '2026-04-27 11:46:29.168482+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1759, 1745, 'default', '2026-04-27 11:46:29.171501+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1800, 1786, 'default', '2026-04-27 11:46:29.187662+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1790, 1776, 'default', '2026-04-27 11:46:29.202087+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1791, 1777, 'default', '2026-04-27 11:46:29.21106+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1811, 1797, 'default', '2026-04-27 11:49:14.147652+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1577, 1577, 'default', '2026-04-27 11:46:06.951641+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1638, 1638, 'default', '2026-04-27 11:46:18.228423+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1608, 1608, 'default', '2026-04-27 11:46:18.24407+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1661, 1661, 'default', '2026-04-27 11:46:27.031146+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1557, 1557, 'default', '2026-04-27 11:45:16.696582+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1671, 1671, 'default', '2026-04-27 11:46:29.074292+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1582, 1582, 'default', '2026-04-27 11:45:38.024953+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1742, 1728, 'default', '2026-04-27 11:46:29.082894+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1489, 1489, 'default', '2026-04-27 11:45:46.897893+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1519, 1519, 'default', '2026-04-27 11:45:48.061131+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1743, 1729, 'default', '2026-04-27 11:46:29.093507+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1713, 1699, 'default', '2026-04-27 11:46:29.096425+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1754, 1740, 'default', '2026-04-27 11:46:29.162573+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1786, 1772, 'default', '2026-04-27 11:46:29.1739+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1760, 1746, 'default', '2026-04-27 11:46:29.176732+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1801, 1787, 'default', '2026-04-27 11:46:29.193426+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1793, 1779, 'default', '2026-04-27 11:46:29.23224+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1584, 1584, 'default', '2026-04-27 11:45:56.903952+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1578, 1578, 'default', '2026-04-27 11:46:06.997806+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1609, 1609, 'default', '2026-04-27 11:46:18.244111+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1639, 1639, 'default', '2026-04-27 11:46:27.031872+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1663, 1663, 'default', '2026-04-27 11:46:27.06089+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1672, 1672, 'default', '2026-04-27 11:46:29.087167+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1714, 1700, 'default', '2026-04-27 11:46:29.099453+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1794, 1780, 'default', '2026-04-27 11:44:55.902429+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1744, 1730, 'default', '2026-04-27 11:46:29.109707+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1755, 1741, 'default', '2026-04-27 11:46:29.15959+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1558, 1558, 'default', '2026-04-27 11:45:17.758268+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1583, 1583, 'default', '2026-04-27 11:45:38.866837+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1459, 1459, 'default', '2026-04-27 11:45:48.887984+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1787, 1773, 'default', '2026-04-27 11:46:29.182479+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1761, 1747, 'default', '2026-04-27 11:46:29.184813+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1802, 1788, 'default', '2026-04-27 11:46:29.199472+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1803, 1789, 'default', '2026-04-27 11:46:29.208021+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1804, 1790, 'default', '2026-04-27 11:46:29.214309+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1543, 1543, 'default', '2026-04-27 11:45:56.903979+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1585, 1585, 'default', '2026-04-27 11:45:58.141518+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1795, 1781, 'default', '2026-04-27 11:44:55.907027+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1610, 1610, 'default', '2026-04-27 11:46:18.989373+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1640, 1640, 'default', '2026-04-27 11:46:27.045881+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1664, 1664, 'default', '2026-04-27 11:46:28.338745+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1756, 1742, 'default', '2026-04-27 11:44:58.639064+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1673, 1673, 'default', '2026-04-27 11:46:29.090321+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1559, 1559, 'default', '2026-04-27 11:45:18.75364+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1745, 1731, 'default', '2026-04-27 11:46:29.106851+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1715, 1701, 'default', '2026-04-27 11:46:29.112948+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1561, 1561, 'default', '2026-04-27 11:45:27.968003+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1579, 1579, 'default', '2026-04-27 11:45:38.024788+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1788, 1774, 'default', '2026-04-27 11:46:29.190509+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1762, 1748, 'default', '2026-04-27 11:46:29.205672+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1805, 1791, 'default', '2026-04-27 11:46:29.222026+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1544, 1544, 'default', '2026-04-27 11:45:56.916886+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1586, 1586, 'default', '2026-04-27 11:45:58.835951+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1806, 1792, 'default', '2026-04-27 11:44:57.611266+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1796, 1782, 'default', '2026-04-27 11:44:58.625857+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1611, 1611, 'default', '2026-04-27 11:46:27.045705+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1641, 1641, 'default', '2026-04-27 11:46:27.053614+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1665, 1665, 'default', '2026-04-27 11:46:29.028497+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1674, 1674, 'default', '2026-04-27 11:46:29.103431+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1807, 1793, 'default', '2026-04-27 11:45:16.683668+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1560, 1560, 'default', '2026-04-27 11:45:17.758171+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1757, 1743, 'default', '2026-04-27 11:45:46.862301+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1716, 1702, 'default', '2026-04-27 11:46:29.123863+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1746, 1732, 'default', '2026-04-27 11:46:29.126395+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1677, 1677, 'default', '2026-04-27 11:46:29.150452+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1763, 1749, 'default', '2026-04-27 11:46:29.228155+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1764, 1750, 'default', '2026-04-27 11:44:55.903138+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1797, 1783, 'default', '2026-04-27 11:45:16.697114+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1545, 1545, 'default', '2026-04-27 11:45:58.925088+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1587, 1587, 'default', '2026-04-27 11:46:06.936954+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1612, 1612, 'default', '2026-04-27 11:46:27.0464+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1562, 1562, 'default', '2026-04-27 11:45:26.76679+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1642, 1642, 'default', '2026-04-27 11:46:27.093861+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1666, 1666, 'default', '2026-04-27 11:46:29.048751+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1675, 1675, 'default', '2026-04-27 11:46:29.119612+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1747, 1733, 'default', '2026-04-27 11:46:29.132926+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1717, 1703, 'default', '2026-04-27 11:46:29.147053+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1765, 1751, 'default', '2026-04-27 11:44:55.902405+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1667, 1667, 'default', '2026-04-27 11:45:58.134645+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1546, 1546, 'default', '2026-04-27 11:46:06.943752+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1563, 1563, 'default', '2026-04-27 11:45:36.792139+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1588, 1588, 'default', '2026-04-27 11:46:06.966226+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1564, 1564, 'default', '2026-04-27 11:45:36.806892+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1613, 1613, 'default', '2026-04-27 11:46:27.059847+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1643, 1643, 'default', '2026-04-27 11:46:27.086502+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1676, 1676, 'default', '2026-04-27 11:46:29.129808+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1748, 1734, 'default', '2026-04-27 11:46:29.138505+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1718, 1704, 'default', '2026-04-27 11:46:29.141476+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1547, 1547, 'default', '2026-04-27 11:45:58.142568+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1766, 1752, 'default', '2026-04-27 11:44:55.903145+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1589, 1589, 'default', '2026-04-27 11:46:16.999891+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1614, 1614, 'default', '2026-04-27 11:46:27.038107+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1719, 1705, 'default', '2026-04-27 11:44:56.026807+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1644, 1644, 'default', '2026-04-27 11:46:27.108923+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1749, 1735, 'default', '2026-04-27 11:44:59.639725+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1565, 1565, 'default', '2026-04-27 11:45:27.968004+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1668, 1668, 'default', '2026-04-27 11:46:29.058997+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1678, 1678, 'default', '2026-04-27 11:46:29.153367+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1590, 1590, 'default', '2026-04-27 11:46:06.966493+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1767, 1753, 'default', '2026-04-27 11:44:55.905823+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1591, 1591, 'default', '2026-04-27 11:46:08.170259+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1679, 1679, 'default', '2026-04-27 11:44:55.921988+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1681, 1681, 'default', '2026-04-27 11:44:56.026812+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1720, 1706, 'default', '2026-04-27 11:44:56.02814+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1566, 1566, 'default', '2026-04-27 11:45:46.862264+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1548, 1548, 'default', '2026-04-27 11:46:08.183865+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1615, 1615, 'default', '2026-04-27 11:46:27.067336+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1645, 1645, 'default', '2026-04-27 11:46:27.111946+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1594, 1594, 'default', '2026-04-27 11:46:08.863564+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1768, 1754, 'default', '2026-04-27 11:44:55.905406+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1680, 1680, 'default', '2026-04-27 11:44:56.027043+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1592, 1592, 'default', '2026-04-27 11:46:16.984425+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1616, 1616, 'default', '2026-04-27 11:46:27.046649+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1721, 1707, 'default', '2026-04-27 11:44:56.046212+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1549, 1549, 'default', '2026-04-27 11:44:56.096689+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1567, 1567, 'default', '2026-04-27 11:45:40.001707+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1650, 1650, 'default', '2026-04-27 11:46:27.10509+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1646, 1646, 'default', '2026-04-27 11:46:27.121721+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1593, 1593, 'default', '2026-04-27 11:46:17.000422+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1769, 1755, 'default', '2026-04-27 11:44:55.904999+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1722, 1708, 'default', '2026-04-27 11:44:56.02714+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1682, 1682, 'default', '2026-04-27 11:44:56.027436+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1647, 1647, 'default', '2026-04-27 11:46:27.101949+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1620, 1620, 'default', '2026-04-27 11:46:27.118679+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1550, 1550, 'default', '2026-04-27 11:44:56.097448+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1617, 1617, 'default', '2026-04-27 11:46:27.130781+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1553, 1553, 'default', '2026-04-27 11:45:06.632868+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1568, 1568, 'default', '2026-04-27 11:45:36.807347+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1570, 1570, 'default', '2026-04-27 11:45:46.861911+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1595, 1595, 'default', '2026-04-27 11:46:08.183981+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1770, 1756, 'default', '2026-04-27 11:44:55.902924+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1683, 1683, 'default', '2026-04-27 11:44:56.027116+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1596, 1596, 'default', '2026-04-27 11:46:16.983723+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1618, 1618, 'default', '2026-04-27 11:46:27.093842+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1723, 1709, 'default', '2026-04-27 11:44:56.0403+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1551, 1551, 'default', '2026-04-27 11:44:56.11655+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1569, 1569, 'default', '2026-04-27 11:45:46.862264+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1648, 1648, 'default', '2026-04-27 11:46:27.098599+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1619, 1619, 'default', '2026-04-27 11:46:28.307877+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1597, 1597, 'default', '2026-04-27 11:46:08.955302+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1771, 1757, 'default', '2026-04-27 11:44:55.905109+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1621, 1621, 'default', '2026-04-27 11:46:27.061795+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1685, 1683, 'default', '2026-04-27 11:44:56.027063+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1684, 1682, 'default', '2026-04-27 11:44:56.028342+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1649, 1649, 'default', '2026-04-27 11:46:28.307162+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1724, 1710, 'default', '2026-04-27 11:44:56.028179+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1687, 1684, 'default', '2026-04-27 11:44:56.028413+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1552, 1552, 'default', '2026-04-27 11:44:58.644843+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1571, 1571, 'default', '2026-04-27 11:45:36.854514+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1598, 1598, 'default', '2026-04-27 11:46:10.119677+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1772, 1758, 'default', '2026-04-27 11:44:55.904858+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1725, 1711, 'default', '2026-04-27 11:44:56.027013+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1651, 1651, 'default', '2026-04-27 11:46:27.115712+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1622, 1622, 'default', '2026-04-27 11:46:27.155171+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1686, 1683, 'default', '2026-04-27 11:44:56.046189+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1554, 1554, 'default', '2026-04-27 11:45:06.632866+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1572, 1572, 'default', '2026-04-27 11:45:38.851659+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1624, 1624, 'default', '2026-04-27 11:46:28.307873+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1599, 1599, 'default', '2026-04-27 11:46:17.043615+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1773, 1759, 'default', '2026-04-27 11:44:55.902202+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1652, 1652, 'default', '2026-04-27 11:46:27.125953+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1623, 1623, 'default', '2026-04-27 11:46:28.295708+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1726, 1712, 'default', '2026-04-27 11:44:55.9024+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1688, 1682, 'default', '2026-04-27 11:44:56.027162+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1573, 1573, 'default', '2026-04-27 11:45:46.862296+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1626, 1626, 'default', '2026-04-27 11:46:28.348585+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1600, 1600, 'default', '2026-04-27 11:46:17.000429+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1774, 1760, 'default', '2026-04-27 11:44:55.904617+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1653, 1653, 'default', '2026-04-27 11:46:28.289983+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1727, 1713, 'default', '2026-04-27 11:44:56.027577+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1689, 1684, 'default', '2026-04-27 11:44:56.050594+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1627, 1627, 'default', '2026-04-27 11:46:28.97872+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1695, 1686, 'default', '2026-04-27 11:44:56.053909+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1628, 1628, 'default', '2026-04-27 11:44:56.119042+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1574, 1574, 'default', '2026-04-27 11:45:48.763687+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1625, 1625, 'default', '2026-04-27 11:46:29.035058+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1601, 1601, 'default', '2026-04-27 11:46:17.000421+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1775, 1761, 'default', '2026-04-27 11:44:55.904468+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1728, 1714, 'default', '2026-04-27 11:44:56.028428+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1690, 1685, 'default', '2026-04-27 11:44:56.064201+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1629, 1629, 'default', '2026-04-27 11:44:57.613711+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1654, 1654, 'default', '2026-04-27 11:46:28.973106+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1602, 1602, 'default', '2026-04-27 11:46:19.013795+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1655, 1655, 'default', '2026-04-27 11:46:28.967598+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1776, 1762, 'default', '2026-04-27 11:44:55.91015+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1691, 1682, 'default', '2026-04-27 11:44:56.048043+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1729, 1715, 'default', '2026-04-27 11:44:56.055748+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1630, 1630, 'default', '2026-04-27 11:44:58.632444+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1603, 1603, 'default', '2026-04-27 11:46:17.016997+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1777, 1763, 'default', '2026-04-27 11:44:55.905775+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1693, 1684, 'default', '2026-04-27 11:44:56.046716+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1694, 1685, 'default', '2026-04-27 11:44:56.05251+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1656, 1656, 'default', '2026-04-27 11:46:28.324268+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1692, 1683, 'default', '2026-04-27 11:44:56.053358+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1730, 1716, 'default', '2026-04-27 11:44:56.055894+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1631, 1631, 'default', '2026-04-27 11:44:56.097823+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1632, 1632, 'default', '2026-04-27 11:44:56.099891+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1633, 1633, 'default', '2026-04-27 11:44:57.612144+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1604, 1604, 'default', '2026-04-27 11:46:17.048324+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1657, 1657, 'default', '2026-04-27 11:46:29.028026+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1778, 1764, 'default', '2026-04-27 11:44:55.90891+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1731, 1717, 'default', '2026-04-27 11:44:56.047058+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1696, 1685, 'default', '2026-04-27 11:44:56.052478+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1701, 1682, 'default', '2026-04-27 11:44:56.068266+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1634, 1634, 'default', '2026-04-27 11:45:16.697103+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1779, 1765, 'default', '2026-04-27 11:44:56.026961+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1658, 1658, 'default', '2026-04-27 11:46:06.936957+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1732, 1718, 'default', '2026-04-27 11:44:56.029463+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1697, 1687, 'default', '2026-04-27 11:44:56.02827+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1704, 1690, 'default', '2026-04-27 11:44:56.07114+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1605, 1605, 'default', '2026-04-27 11:44:56.105481+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1452, 1452, 'default', '2026-04-27 11:45:38.025566+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1456, 1456, 'default', '2026-04-27 11:45:38.728808+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1780, 1766, 'default', '2026-04-27 11:44:56.009336+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1700, 1688, 'default', '2026-04-27 11:44:56.052437+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1698, 1686, 'default', '2026-04-27 11:44:56.054364+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1462, 1462, 'default', '2026-04-27 11:46:06.936726+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1733, 1719, 'default', '2026-04-27 11:44:56.072298+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1478, 1478, 'default', '2026-04-27 11:45:28.805945+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1493, 1493, 'default', '2026-04-27 11:46:08.970256+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1492, 1492, 'default', '2026-04-27 11:45:58.141843+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1781, 1767, 'default', '2026-04-27 11:44:56.027109+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1699, 1684, 'default', '2026-04-27 11:44:56.050537+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1734, 1720, 'default', '2026-04-27 11:44:56.074085+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1539, 1539, 'default', '2026-04-27 11:45:28.822859+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1513, 1513, 'default', '2026-04-27 11:45:46.862291+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1454, 1454, 'default', '2026-04-27 11:45:48.060496+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1455, 1455, 'default', '2026-04-27 11:45:48.061132+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1702, 1683, 'default', '2026-04-27 11:44:56.054196+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1735, 1721, 'default', '2026-04-27 11:44:56.074083+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1509, 1509, 'default', '2026-04-27 11:45:27.967452+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1580, 1580, 'default', '2026-04-27 11:45:38.851503+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1516, 1516, 'default', '2026-04-27 11:45:48.902137+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1703, 1689, 'default', '2026-04-27 11:44:56.069926+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1736, 1722, 'default', '2026-04-27 11:44:56.072777+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1738, 1724, 'default', '2026-04-27 11:44:56.103737+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1508, 1508, 'default', '2026-04-27 11:45:26.781745+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1451, 1451, 'default', '2026-04-27 11:45:27.968001+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1460, 1460, 'default', '2026-04-27 11:45:56.949192+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1705, 1691, 'default', '2026-04-27 11:44:56.073044+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1737, 1723, 'default', '2026-04-27 11:44:56.084991+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1479, 1479, 'default', '2026-04-27 11:45:26.812792+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1490, 1490, 'default', '2026-04-27 11:45:56.904077+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1522, 1522, 'default', '2026-04-27 11:46:06.951138+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1706, 1692, 'default', '2026-04-27 11:44:56.077984+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1449, 1449, 'default', '2026-04-27 11:45:26.766778+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1482, 1482, 'default', '2026-04-27 11:45:36.807223+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1515, 1515, 'default', '2026-04-27 11:45:38.025521+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1523, 1523, 'default', '2026-04-27 11:46:06.951665+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1707, 1693, 'default', '2026-04-27 11:44:56.077018+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1540, 1540, 'default', '2026-04-27 11:45:36.792181+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1512, 1512, 'default', '2026-04-27 11:45:36.806888+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1486, 1486, 'default', '2026-04-27 11:45:46.872909+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1708, 1694, 'default', '2026-04-27 11:45:06.632849+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1485, 1485, 'default', '2026-04-27 11:45:56.903804+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1510, 1510, 'default', '2026-04-27 11:45:29.965961+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1463, 1463, 'default', '2026-04-27 11:46:08.183869+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1484, 1484, 'default', '2026-04-27 11:45:56.903886+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1480, 1480, 'default', '2026-04-27 11:45:28.680279+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1520, 1520, 'default', '2026-04-27 11:45:58.134409+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1450, 1450, 'default', '2026-04-27 11:45:36.792138+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1521, 1521, 'default', '2026-04-27 11:46:06.95164+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1541, 1541, 'default', '2026-04-27 11:45:28.806322+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1542, 1542, 'default', '2026-04-27 11:45:46.862126+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1458, 1458, 'default', '2026-04-27 11:45:56.90402+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1511, 1511, 'default', '2026-04-27 11:45:36.792157+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1575, 1575, 'default', '2026-04-27 11:45:58.940394+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1481, 1481, 'default', '2026-04-27 11:45:36.791913+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1483, 1483, 'default', '2026-04-27 11:45:36.823093+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1453, 1453, 'default', '2026-04-27 11:45:46.862216+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1491, 1491, 'default', '2026-04-27 11:46:00.07939+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1517, 1517, 'default', '2026-04-27 11:45:46.868062+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1457, 1457, 'default', '2026-04-27 11:45:48.061139+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1487, 1487, 'default', '2026-04-27 11:45:50.039955+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1461, 1461, 'default', '2026-04-27 11:45:58.92509+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1514, 1514, 'default', '2026-04-27 11:45:56.903862+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1464, 1464, 'default', '2026-04-27 11:46:08.955552+00', 0);
INSERT INTO hangfire.jobqueue (id, jobid, queue, fetchedat, updatecount) VALUES (1488, 1488, 'default', '2026-04-27 11:45:48.887671+00', 0);


--
-- TOC entry 3989 (class 0 OID 16922)
-- Dependencies: 269
-- Data for Name: list; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--



--
-- TOC entry 3995 (class 0 OID 16963)
-- Dependencies: 275
-- Data for Name: lock; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--



--
-- TOC entry 3977 (class 0 OID 16864)
-- Dependencies: 257
-- Data for Name: schema; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.schema (version) VALUES (23);


--
-- TOC entry 3990 (class 0 OID 16930)
-- Dependencies: 270
-- Data for Name: server; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.server (id, data, lastheartbeat, updatecount) VALUES ('desktop-o2bpcl9:24304:4983756f-ca44-41c0-9bee-33cb991a76c3', '{"Queues": ["default"], "StartedAt": "2026-04-27T11:44:55.632805Z", "WorkerCount": 20}', '2026-04-27 11:48:25.782935+00', 0);
INSERT INTO hangfire.server (id, data, lastheartbeat, updatecount) VALUES ('desktop-o2bpcl9:35540:d730349e-25da-41f3-8a12-86c9f4613787', '{"Queues": ["default"], "StartedAt": "2026-04-27T11:48:58.7353371Z", "WorkerCount": 20}', '2026-04-27 11:49:28.77281+00', 0);


--
-- TOC entry 3992 (class 0 OID 16938)
-- Dependencies: 272
-- Data for Name: set; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.set (id, key, score, value, expireat, updatecount) VALUES (1, 'recurring-jobs', 1777290600, 'order-assembly-planner', NULL, 0);


--
-- TOC entry 3985 (class 0 OID 16899)
-- Dependencies: 265
-- Data for Name: state; Type: TABLE DATA; Schema: hangfire; Owner: taskservice_user
--

INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4939, 1669, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:11:07.143595+00', '{"Queue": "default", "EnqueuedAt": "1777270267143"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5078, 1697, 'Processing', NULL, '2026-04-27 06:43:03.518511+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272183515"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4929, 1665, 'Succeeded', NULL, '2026-04-26 12:33:12.145067+00', '{"Latency": "-32399968", "SucceededAt": "1777206792112", "PerformanceDuration": "89"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5084, 1699, 'Processing', NULL, '2026-04-27 06:45:03.645563+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272303640"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5090, 1701, 'Processing', NULL, '2026-04-27 06:47:03.781988+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272423778"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5204, 1738, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:49:05.119649+00', '{"Queue": "default", "EnqueuedAt": "1777276145119"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5322, 1777, 'Processing', NULL, '2026-04-27 08:58:11.534545+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280291531"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5326, 1778, 'Succeeded', NULL, '2026-04-27 08:59:11.68494+00', '{"Latency": "-32399981", "SucceededAt": "1777280351678", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4940, 1669, 'Processing', NULL, '2026-04-27 06:11:07.157383+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "8d216711-88e4-4de7-945f-c2f006a4f5b6", "StartedAt": "1777270267153"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5079, 1697, 'Succeeded', NULL, '2026-04-27 06:43:03.576722+00', '{"Latency": "-32399981", "SucceededAt": "1777272183571", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5081, 1698, 'Processing', NULL, '2026-04-27 06:44:03.582347+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272243578"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4930, 1666, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:34:12.880161+00', '{"Queue": "default", "EnqueuedAt": "1777206852879"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5089, 1701, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:47:03.770551+00', '{"Queue": "default", "EnqueuedAt": "1777272423770"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5205, 1738, 'Processing', NULL, '2026-04-27 07:49:05.13293+00', '{"ServerId": "desktop-o2bpcl9:14740:2b039c95-450f-4c00-bc50-a29d0039c747", "WorkerId": "9a3f07ca-df24-416b-9b6c-4c86ec096410", "StartedAt": "1777276145129"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5328, 1779, 'Processing', NULL, '2026-04-27 09:00:11.688394+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280411685"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5329, 1779, 'Succeeded', NULL, '2026-04-27 09:00:11.741642+00', '{"Latency": "-32399983", "SucceededAt": "1777280411737", "PerformanceDuration": "45"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4941, 1669, 'Succeeded', NULL, '2026-04-27 06:11:07.248934+00', '{"Latency": "-32399978", "SucceededAt": "1777270267241", "PerformanceDuration": "80"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4948, 1672, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:14:07.445091+00', '{"Queue": "default", "EnqueuedAt": "1777270447444"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5082, 1698, 'Succeeded', NULL, '2026-04-27 06:44:03.639803+00', '{"Latency": "-32399983", "SucceededAt": "1777272243634", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4933, 1667, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:09:51.880954+00', '{"Queue": "default", "EnqueuedAt": "1777270191868"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5206, 1738, 'Succeeded', NULL, '2026-04-27 07:49:05.21293+00', '{"Latency": "-32399978", "SucceededAt": "1777276145206", "PerformanceDuration": "69"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5330, 1780, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:01:11.73538+00', '{"Queue": "default", "EnqueuedAt": "1777280471735"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5332, 1780, 'Succeeded', NULL, '2026-04-27 09:01:11.795727+00', '{"Latency": "-32399983", "SucceededAt": "1777280471791", "PerformanceDuration": "42"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4942, 1670, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:12:07.25999+00', '{"Queue": "default", "EnqueuedAt": "1777270327259"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4934, 1667, 'Processing', NULL, '2026-04-27 06:09:51.953779+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "64f9cdbb-e48d-4a4b-bb17-3b338fbef91f", "StartedAt": "1777270191940"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5083, 1699, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:45:03.630567+00', '{"Queue": "default", "EnqueuedAt": "1777272303630"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5087, 1700, 'Processing', NULL, '2026-04-27 06:46:03.711428+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272363707"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5091, 1701, 'Succeeded', NULL, '2026-04-27 06:47:03.845376+00', '{"Latency": "-32399981", "SucceededAt": "1777272423839", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5207, 1739, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:50:05.215968+00', '{"Queue": "default", "EnqueuedAt": "1777276205215"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5333, 1781, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:08:44.361859+00', '{"Queue": "default", "EnqueuedAt": "1777280924342"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4943, 1670, 'Processing', NULL, '2026-04-27 06:12:07.280674+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270327273"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4935, 1667, 'Succeeded', NULL, '2026-04-27 06:09:52.956396+00', '{"Latency": "-32399780", "SucceededAt": "1777270192943", "PerformanceDuration": "967"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5085, 1699, 'Succeeded', NULL, '2026-04-27 06:45:03.724808+00', '{"Latency": "-32399975", "SucceededAt": "1777272303718", "PerformanceDuration": "69"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5086, 1700, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:46:03.701084+00', '{"Queue": "default", "EnqueuedAt": "1777272363700"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5208, 1739, 'Processing', NULL, '2026-04-27 07:50:05.226825+00', '{"ServerId": "desktop-o2bpcl9:14740:2b039c95-450f-4c00-bc50-a29d0039c747", "WorkerId": "9a3f07ca-df24-416b-9b6c-4c86ec096410", "StartedAt": "1777276205223"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5334, 1781, 'Processing', NULL, '2026-04-27 09:08:44.411002+00', '{"ServerId": "desktop-o2bpcl9:35816:fde4f976-bfd6-46db-a26f-6c2c56116a78", "WorkerId": "3b5faf28-4137-4dee-996d-1f05effdf5eb", "StartedAt": "1777280924401"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4944, 1670, 'Succeeded', NULL, '2026-04-27 06:12:07.372956+00', '{"Latency": "-32399961", "SucceededAt": "1777270327367", "PerformanceDuration": "76"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4936, 1668, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:10:06.994681+00', '{"Queue": "default", "EnqueuedAt": "1777270206994"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5088, 1700, 'Succeeded', NULL, '2026-04-27 06:46:03.769425+00', '{"Latency": "-32399983", "SucceededAt": "1777272363764", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5209, 1739, 'Succeeded', NULL, '2026-04-27 07:50:05.314804+00', '{"Latency": "-32399981", "SucceededAt": "1777276205309", "PerformanceDuration": "79"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5335, 1781, 'Succeeded', NULL, '2026-04-27 09:08:45.934021+00', '{"Latency": "-32399864", "SucceededAt": "1777280925917", "PerformanceDuration": "1494"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4945, 1671, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:13:07.361143+00', '{"Queue": "default", "EnqueuedAt": "1777270387361"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5092, 1702, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:48:03.860686+00', '{"Queue": "default", "EnqueuedAt": "1777272483860"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4937, 1668, 'Processing', NULL, '2026-04-27 06:10:07.011372+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270207007"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5210, 1740, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:51:22.636524+00', '{"Queue": "default", "EnqueuedAt": "1777276282627"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5336, 1782, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:21:49.961895+00', '{"Queue": "default", "EnqueuedAt": "1777281709945"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4946, 1671, 'Processing', NULL, '2026-04-27 06:13:07.375464+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270387371"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4938, 1668, 'Succeeded', NULL, '2026-04-27 06:10:07.103963+00', '{"Latency": "-32399971", "SucceededAt": "1777270207092", "PerformanceDuration": "75"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5093, 1702, 'Processing', NULL, '2026-04-27 06:48:03.871302+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272483868"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5211, 1740, 'Processing', NULL, '2026-04-27 07:51:22.690102+00', '{"ServerId": "desktop-o2bpcl9:19856:b037e4b9-3f91-4edb-8abb-0385c04f68c7", "WorkerId": "df5ef6fc-6e1d-4e6c-89f0-5ebf3b65c8cf", "StartedAt": "1777276282670"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5337, 1782, 'Processing', NULL, '2026-04-27 09:21:50.909165+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "9595fcb0-47e4-4213-8160-eab8cf4c320f", "StartedAt": "1777281710894"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5340, 1783, 'Processing', NULL, '2026-04-27 09:22:10.048165+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "671c74c6-578b-4c84-9dc1-2a39db546bbd", "StartedAt": "1777281730042"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5344, 1784, 'Succeeded', NULL, '2026-04-27 09:23:10.428642+00', '{"Latency": "-32394801", "SucceededAt": "1777281790421", "PerformanceDuration": "70"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5350, 1786, 'Succeeded', NULL, '2026-04-27 09:25:05.395615+00', '{"Latency": "-32399977", "SucceededAt": "1777281905389", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4947, 1671, 'Succeeded', NULL, '2026-04-27 06:13:07.461454+00', '{"Latency": "-32399974", "SucceededAt": "1777270387456", "PerformanceDuration": "74"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5094, 1702, 'Succeeded', NULL, '2026-04-27 06:48:03.928287+00', '{"Latency": "-32399982", "SucceededAt": "1777272483922", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5212, 1740, 'Succeeded', NULL, '2026-04-27 07:51:23.928582+00', '{"Latency": "-32399898", "SucceededAt": "1777276283913", "PerformanceDuration": "1211"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5338, 1782, 'Succeeded', NULL, '2026-04-27 09:21:51.795529+00', '{"Latency": "-32398916", "SucceededAt": "1777281711777", "PerformanceDuration": "803"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4949, 1672, 'Processing', NULL, '2026-04-27 06:14:07.458935+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270447455"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5095, 1703, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:49:03.958842+00', '{"Queue": "default", "EnqueuedAt": "1777272543958"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5213, 1741, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:52:14.59389+00', '{"Queue": "default", "EnqueuedAt": "1777276334581"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5339, 1783, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:22:05.032216+00', '{"Queue": "default", "EnqueuedAt": "1777281725031"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5346, 1785, 'Processing', NULL, '2026-04-27 09:24:05.252726+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777281845249"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4950, 1672, 'Succeeded', NULL, '2026-04-27 06:14:07.543802+00', '{"Latency": "-32399978", "SucceededAt": "1777270447537", "PerformanceDuration": "73"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5096, 1703, 'Processing', NULL, '2026-04-27 06:49:03.971279+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272543967"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5099, 1704, 'Processing', NULL, '2026-04-27 06:50:04.043605+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272604039"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5106, 1706, 'Succeeded', NULL, '2026-04-27 06:52:04.278576+00', '{"Latency": "-32399978", "SucceededAt": "1777272724270", "PerformanceDuration": "59"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5107, 1680, 'Processing', NULL, '2026-04-27 06:52:09.009124+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "d74c08b6-2f44-4d8f-99b9-98c845aef1ec", "StartedAt": "1777272729005"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5214, 1741, 'Processing', NULL, '2026-04-27 07:52:14.638984+00', '{"ServerId": "desktop-o2bpcl9:31680:910ce17c-228b-4a25-8293-190f54bac94a", "WorkerId": "f0a3c746-9f94-476c-851c-3d8007f2a197", "StartedAt": "1777276334624"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5341, 1783, 'Succeeded', NULL, '2026-04-27 09:22:10.1525+00', '{"Latency": "-32394969", "SucceededAt": "1777281730146", "PerformanceDuration": "93"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4951, 1673, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:15:07.538211+00', '{"Queue": "default", "EnqueuedAt": "1777270507538"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5097, 1703, 'Succeeded', NULL, '2026-04-27 06:49:04.035767+00', '{"Latency": "-32399977", "SucceededAt": "1777272544029", "PerformanceDuration": "52"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5215, 1741, 'Succeeded', NULL, '2026-04-27 07:52:15.032976+00', '{"Latency": "-32399910", "SucceededAt": "1777276335019", "PerformanceDuration": "374"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5342, 1784, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:23:05.156738+00', '{"Queue": "default", "EnqueuedAt": "1777281785156"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5348, 1786, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:25:05.321172+00', '{"Queue": "default", "EnqueuedAt": "1777281905321"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4952, 1673, 'Processing', NULL, '2026-04-27 06:15:07.556375+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270507551"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5098, 1704, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:50:04.027893+00', '{"Queue": "default", "EnqueuedAt": "1777272604027"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4866, 1644, 'Succeeded', NULL, '2026-04-26 11:50:05.245635+00', '{"Latency": "-32399967", "SucceededAt": "1777204205241", "PerformanceDuration": "58"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4881, 1649, 'Succeeded', NULL, '2026-04-26 11:55:12.97596+00', '{"Latency": "-32399977", "SucceededAt": "1777204512971", "PerformanceDuration": "100"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4884, 1650, 'Succeeded', NULL, '2026-04-26 11:56:13.025882+00', '{"Latency": "-32399963", "SucceededAt": "1777204573020", "PerformanceDuration": "66"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5103, 1705, 'Succeeded', NULL, '2026-04-27 06:51:04.17459+00', '{"Latency": "-32399980", "SucceededAt": "1777272664169", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5216, 1742, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:19:28.260772+00', '{"Queue": "default", "EnqueuedAt": "1777277968250"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5226, 1745, 'Processing', NULL, '2026-04-27 08:22:13.499122+00', '{"ServerId": "desktop-o2bpcl9:23332:479b6f79-be7e-4049-a342-391746b76a08", "WorkerId": "6175ae51-44c5-4306-b647-595a8ae3ce23", "StartedAt": "1777278133494"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5343, 1784, 'Processing', NULL, '2026-04-27 09:23:10.347186+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "2aa73b70-65ac-4e7c-a991-be501f052961", "StartedAt": "1777281790343"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4953, 1673, 'Succeeded', NULL, '2026-04-27 06:15:07.633505+00', '{"Latency": "-32399971", "SucceededAt": "1777270507626", "PerformanceDuration": "65"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4955, 1674, 'Processing', NULL, '2026-04-27 06:16:07.637179+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270567633"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4865, 1644, 'Processing', NULL, '2026-04-26 11:50:05.16711+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204205163"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4957, 1675, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:17:07.686244+00', '{"Queue": "default", "EnqueuedAt": "1777270627686"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5100, 1704, 'Succeeded', NULL, '2026-04-27 06:50:04.105543+00', '{"Latency": "-32399975", "SucceededAt": "1777272604098", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5217, 1742, 'Processing', NULL, '2026-04-27 08:19:29.197153+00', '{"ServerId": "desktop-o2bpcl9:23332:479b6f79-be7e-4049-a342-391746b76a08", "WorkerId": "9c8ab708-716b-463d-90e4-e4998d455513", "StartedAt": "1777277969192"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5345, 1785, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:24:05.241478+00', '{"Queue": "default", "EnqueuedAt": "1777281845241"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4954, 1674, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:16:07.62432+00', '{"Queue": "default", "EnqueuedAt": "1777270567624"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4963, 1677, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:19:07.930679+00', '{"Queue": "default", "EnqueuedAt": "1777270747930"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5101, 1705, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:51:04.104613+00', '{"Queue": "default", "EnqueuedAt": "1777272664104"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5218, 1742, 'Succeeded', NULL, '2026-04-27 08:19:31.118675+00', '{"Latency": "-32398986", "SucceededAt": "1777277971104", "PerformanceDuration": "1900"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5347, 1785, 'Succeeded', NULL, '2026-04-27 09:24:05.310566+00', '{"Latency": "-32399982", "SucceededAt": "1777281845303", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4956, 1674, 'Succeeded', NULL, '2026-04-27 06:16:07.708962+00', '{"Latency": "-32399980", "SucceededAt": "1777270567702", "PerformanceDuration": "62"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4863, 1643, 'Succeeded', NULL, '2026-04-26 11:49:05.310177+00', '{"Latency": "-32399552", "SucceededAt": "1777204145269", "PerformanceDuration": "122"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4961, 1676, 'Processing', NULL, '2026-04-27 06:18:07.828403+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270687824"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4964, 1677, 'Processing', NULL, '2026-04-27 06:19:07.940958+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270747937"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5102, 1705, 'Processing', NULL, '2026-04-27 06:51:04.117818+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272664112"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5105, 1706, 'Processing', NULL, '2026-04-27 06:52:04.205721+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272724202"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5219, 1743, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:20:13.336844+00', '{"Queue": "default", "EnqueuedAt": "1777278013336"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5349, 1786, 'Processing', NULL, '2026-04-27 09:25:05.336237+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777281905332"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4861, 1643, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:49:04.736767+00', '{"Queue": "default", "EnqueuedAt": "1777204144736"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4958, 1675, 'Processing', NULL, '2026-04-27 06:17:07.696719+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270627693"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5104, 1706, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:52:04.194195+00', '{"Queue": "default", "EnqueuedAt": "1777272724194"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5220, 1743, 'Processing', NULL, '2026-04-27 08:20:18.507267+00', '{"ServerId": "desktop-o2bpcl9:23332:479b6f79-be7e-4049-a342-391746b76a08", "WorkerId": "2ecfd99f-b547-4b07-b327-d845e1532c8c", "StartedAt": "1777278018501"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5221, 1743, 'Succeeded', NULL, '2026-04-27 08:20:18.588053+00', '{"Latency": "-32394819", "SucceededAt": "1777278018581", "PerformanceDuration": "68"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5228, 1746, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:23:13.536823+00', '{"Queue": "default", "EnqueuedAt": "1777278193536"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5351, 1787, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:26:05.407368+00', '{"Queue": "default", "EnqueuedAt": "1777281965407"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4862, 1643, 'Processing', NULL, '2026-04-26 11:49:04.948538+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204144855"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4959, 1675, 'Succeeded', NULL, '2026-04-27 06:17:07.777601+00', '{"Latency": "-32399982", "SucceededAt": "1777270627772", "PerformanceDuration": "71"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4960, 1676, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:18:07.816302+00', '{"Queue": "default", "EnqueuedAt": "1777270687816"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5108, 1680, 'Succeeded', NULL, '2026-04-27 06:52:09.073329+00', '{"Latency": "-30597238", "SucceededAt": "1777272729067", "PerformanceDuration": "52"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5222, 1744, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:21:13.406955+00', '{"Queue": "default", "EnqueuedAt": "1777278073406"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5231, 1747, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:24:13.588482+00', '{"Queue": "default", "EnqueuedAt": "1777278253588"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5352, 1787, 'Processing', NULL, '2026-04-27 09:26:05.418559+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777281965415"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4864, 1644, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:50:05.155567+00', '{"Queue": "default", "EnqueuedAt": "1777204205155"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4962, 1676, 'Succeeded', NULL, '2026-04-27 06:18:07.902526+00', '{"Latency": "-32399980", "SucceededAt": "1777270687895", "PerformanceDuration": "63"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5109, 1707, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:53:04.28643+00', '{"Queue": "default", "EnqueuedAt": "1777272784286"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5116, 1681, 'Succeeded', NULL, '2026-04-27 06:54:49.515975+00', '{"Latency": "-30599770", "SucceededAt": "1777272889507", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5223, 1744, 'Processing', NULL, '2026-04-27 08:21:13.41954+00', '{"ServerId": "desktop-o2bpcl9:23332:479b6f79-be7e-4049-a342-391746b76a08", "WorkerId": "6175ae51-44c5-4306-b647-595a8ae3ce23", "StartedAt": "1777278073416"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5353, 1787, 'Succeeded', NULL, '2026-04-27 09:26:05.481735+00', '{"Latency": "-32399981", "SucceededAt": "1777281965476", "PerformanceDuration": "54"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4867, 1645, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:51:05.238169+00', '{"Queue": "default", "EnqueuedAt": "1777204265237"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4965, 1677, 'Succeeded', NULL, '2026-04-27 06:19:08.026062+00', '{"Latency": "-32399983", "SucceededAt": "1777270748020", "PerformanceDuration": "75"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5110, 1707, 'Processing', NULL, '2026-04-27 06:53:04.297502+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272784294"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5119, 1709, 'Succeeded', NULL, '2026-04-27 06:55:04.49153+00', '{"Latency": "-32399983", "SucceededAt": "1777272904486", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5123, 1711, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:57:04.580398+00', '{"Queue": "default", "EnqueuedAt": "1777273024580"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5224, 1744, 'Succeeded', NULL, '2026-04-27 08:21:13.511974+00', '{"Latency": "-32399978", "SucceededAt": "1777278073502", "PerformanceDuration": "78"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5230, 1746, 'Succeeded', NULL, '2026-04-27 08:23:13.622646+00', '{"Latency": "-32399980", "SucceededAt": "1777278193617", "PerformanceDuration": "64"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5354, 1788, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:27:05.494344+00', '{"Queue": "default", "EnqueuedAt": "1777282025494"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5357, 1789, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:28:05.575701+00', '{"Queue": "default", "EnqueuedAt": "1777282085575"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5360, 1790, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:29:05.671235+00', '{"Queue": "default", "EnqueuedAt": "1777282145671"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4868, 1645, 'Processing', NULL, '2026-04-26 11:51:05.252656+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204265247"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4966, 1678, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:20:08.018759+00', '{"Queue": "default", "EnqueuedAt": "1777270808018"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5111, 1707, 'Succeeded', NULL, '2026-04-27 06:53:04.353806+00', '{"Latency": "-32399981", "SucceededAt": "1777272784348", "PerformanceDuration": "46"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5225, 1745, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:22:13.483301+00', '{"Queue": "default", "EnqueuedAt": "1777278133483"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4870, 1646, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:52:05.315694+00', '{"Queue": "default", "EnqueuedAt": "1777204325315"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4882, 1650, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:56:12.923011+00', '{"Queue": "default", "EnqueuedAt": "1777204572922"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5355, 1788, 'Processing', NULL, '2026-04-27 09:27:05.509191+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777282025504"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5358, 1789, 'Processing', NULL, '2026-04-27 09:28:05.590365+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777282085585"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4869, 1645, 'Succeeded', NULL, '2026-04-26 11:51:05.340962+00', '{"Latency": "-32399975", "SucceededAt": "1777204265320", "PerformanceDuration": "63"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4967, 1678, 'Processing', NULL, '2026-04-27 06:20:08.040545+00', '{"ServerId": "desktop-o2bpcl9:24956:a945e47f-8f24-49d3-9b32-7c9fff8caf29", "WorkerId": "0b5309e0-935a-424e-8cbe-0d6ea9886f9b", "StartedAt": "1777270808034"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5112, 1708, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:54:04.351641+00', '{"Queue": "default", "EnqueuedAt": "1777272844351"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5121, 1710, 'Processing', NULL, '2026-04-27 06:56:04.523829+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272964520"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4876, 1648, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:54:12.769417+00', '{"Queue": "default", "EnqueuedAt": "1777204452769"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5227, 1745, 'Succeeded', NULL, '2026-04-27 08:22:13.569854+00', '{"Latency": "-32399976", "SucceededAt": "1777278133562", "PerformanceDuration": "59"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5356, 1788, 'Succeeded', NULL, '2026-04-27 09:27:05.569022+00', '{"Latency": "-32399977", "SucceededAt": "1777282025563", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5359, 1789, 'Succeeded', NULL, '2026-04-27 09:28:05.66337+00', '{"Latency": "-32399974", "SucceededAt": "1777282085658", "PerformanceDuration": "60"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4871, 1646, 'Processing', NULL, '2026-04-26 11:52:05.326619+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204325323"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4968, 1678, 'Succeeded', NULL, '2026-04-27 06:20:08.126762+00', '{"Latency": "-32399966", "SucceededAt": "1777270808119", "PerformanceDuration": "72"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5113, 1708, 'Processing', NULL, '2026-04-27 06:54:04.360917+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272844357"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4879, 1649, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:55:12.852469+00', '{"Queue": "default", "EnqueuedAt": "1777204512852"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5117, 1709, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:55:04.424558+00', '{"Queue": "default", "EnqueuedAt": "1777272904424"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5229, 1746, 'Processing', NULL, '2026-04-27 08:23:13.549431+00', '{"ServerId": "desktop-o2bpcl9:23332:479b6f79-be7e-4049-a342-391746b76a08", "WorkerId": "6175ae51-44c5-4306-b647-595a8ae3ce23", "StartedAt": "1777278193544"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5361, 1790, 'Processing', NULL, '2026-04-27 09:29:05.685404+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777282145679"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4872, 1646, 'Succeeded', NULL, '2026-04-26 11:52:39.147521+00', '{"Latency": "-32399973", "SucceededAt": "1777204359142", "PerformanceDuration": "33803"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4969, 1679, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:21:06.132897+00', '{"Queue": "default", "EnqueuedAt": "1777270866120"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4974, 1681, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:24:49.253894+00', '{"Queue": "default", "EnqueuedAt": "1777271089253"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5114, 1708, 'Succeeded', NULL, '2026-04-27 06:54:04.416984+00', '{"Latency": "-32399984", "SucceededAt": "1777272844411", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5232, 1747, 'Processing', NULL, '2026-04-27 08:24:13.601261+00', '{"ServerId": "desktop-o2bpcl9:23332:479b6f79-be7e-4049-a342-391746b76a08", "WorkerId": "6175ae51-44c5-4306-b647-595a8ae3ce23", "StartedAt": "1777278253597"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5362, 1790, 'Succeeded', NULL, '2026-04-27 09:29:05.749501+00', '{"Latency": "-32399978", "SucceededAt": "1777282145743", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5363, 1791, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:30:05.765158+00', '{"Queue": "default", "EnqueuedAt": "1777282205765"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4873, 1647, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:53:09.057319+00', '{"Queue": "default", "EnqueuedAt": "1777204389057"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4970, 1679, 'Processing', NULL, '2026-04-27 06:21:06.201883+00', '{"ServerId": "desktop-o2bpcl9:17788:710c9a89-1241-4c80-a175-0bf8acb8b401", "WorkerId": "c7c34c10-a115-434d-af3f-659d75411f19", "StartedAt": "1777270866170"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5115, 1681, 'Processing', NULL, '2026-04-27 06:54:49.456061+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272889453"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5233, 1747, 'Succeeded', NULL, '2026-04-27 08:24:13.6655+00', '{"Latency": "-32399978", "SucceededAt": "1777278253659", "PerformanceDuration": "52"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5364, 1791, 'Processing', NULL, '2026-04-27 09:30:05.777743+00', '{"ServerId": "desktop-o2bpcl9:35276:dec36af1-36fa-4d24-bca9-b82e830f9044", "WorkerId": "d72073b4-d1bd-4153-a117-5ea6a87aa712", "StartedAt": "1777282205773"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5365, 1791, 'Succeeded', NULL, '2026-04-27 09:30:05.839028+00', '{"Latency": "-32399978", "SucceededAt": "1777282205833", "PerformanceDuration": "51"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4874, 1647, 'Processing', NULL, '2026-04-26 11:53:09.069754+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204389066"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4971, 1679, 'Succeeded', NULL, '2026-04-27 06:21:07.518473+00', '{"Latency": "-32399862", "SucceededAt": "1777270867500", "PerformanceDuration": "1286"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5118, 1709, 'Processing', NULL, '2026-04-27 06:55:04.434733+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272904431"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5125, 1711, 'Succeeded', NULL, '2026-04-27 06:57:04.655639+00', '{"Latency": "-32399979", "SucceededAt": "1777273024649", "PerformanceDuration": "52"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5234, 1748, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:27:44.271861+00', '{"Queue": "default", "EnqueuedAt": "1777278464265"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5366, 1792, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 11:44:56.092053+00', '{"Queue": "default", "EnqueuedAt": "1777290296073"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5368, 1792, 'Succeeded', NULL, '2026-04-27 11:44:58.514342+00', '{"Latency": "-32398292", "SucceededAt": "1777290298502", "PerformanceDuration": "870"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5369, 1793, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 11:45:11.164316+00', '{"Queue": "default", "EnqueuedAt": "1777290311164"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4875, 1647, 'Succeeded', NULL, '2026-04-26 11:53:27.772619+00', '{"Latency": "-32399979", "SucceededAt": "1777204407764", "PerformanceDuration": "18690"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4972, 1680, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:22:06.258242+00', '{"Queue": "default", "EnqueuedAt": "1777270926258"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5120, 1710, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:56:04.512413+00', '{"Queue": "default", "EnqueuedAt": "1777272964512"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5235, 1748, 'Processing', NULL, '2026-04-27 08:27:44.309655+00', '{"ServerId": "desktop-o2bpcl9:37668:2a87f994-371d-4561-b467-49d177fabe34", "WorkerId": "1f2a79d4-b291-4c11-8ee4-98924709b067", "StartedAt": "1777278464296"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5367, 1792, 'Processing', NULL, '2026-04-27 11:44:57.622276+00', '{"ServerId": "desktop-o2bpcl9:24304:4983756f-ca44-41c0-9bee-33cb991a76c3", "WorkerId": "b04f8002-9a1e-481f-870c-4ee7e2c752a7", "StartedAt": "1777290297615"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4877, 1648, 'Processing', NULL, '2026-04-26 11:54:12.779856+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204452776"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4973, 1680, 'Processing', NULL, '2026-04-27 06:22:06.271698+00', '{"ServerId": "desktop-o2bpcl9:17788:710c9a89-1241-4c80-a175-0bf8acb8b401", "WorkerId": "c7c34c10-a115-434d-af3f-659d75411f19", "StartedAt": "1777270926267"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5122, 1710, 'Succeeded', NULL, '2026-04-27 06:56:04.579984+00', '{"Latency": "-32399980", "SucceededAt": "1777272964573", "PerformanceDuration": "46"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5236, 1748, 'Succeeded', NULL, '2026-04-27 08:27:45.549235+00', '{"Latency": "-32399924", "SucceededAt": "1777278465527", "PerformanceDuration": "1211"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5370, 1793, 'Processing', NULL, '2026-04-27 11:45:16.691919+00', '{"ServerId": "desktop-o2bpcl9:24304:4983756f-ca44-41c0-9bee-33cb991a76c3", "WorkerId": "3d77d0c7-5415-4c9a-8556-f9a7d96e2002", "StartedAt": "1777290316686"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4878, 1648, 'Succeeded', NULL, '2026-04-26 11:54:12.975558+00', '{"Latency": "-32399980", "SucceededAt": "1777204452969", "PerformanceDuration": "185"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4975, 1681, 'Processing', NULL, '2026-04-27 06:24:49.279523+00', '{"ServerId": "desktop-o2bpcl9:17788:710c9a89-1241-4c80-a175-0bf8acb8b401", "WorkerId": "980c4c45-69db-4a84-8782-cf17abd63adf", "StartedAt": "1777271089271"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5124, 1711, 'Processing', NULL, '2026-04-27 06:57:04.592685+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273024588"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5237, 1749, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:30:09.328434+00', '{"Queue": "default", "EnqueuedAt": "1777278609318"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5244, 1751, 'Processing', NULL, '2026-04-27 08:32:09.549291+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278729545"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5248, 1752, 'Succeeded', NULL, '2026-04-27 08:33:09.689271+00', '{"Latency": "-32399978", "SucceededAt": "1777278789683", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5371, 1793, 'Succeeded', NULL, '2026-04-27 11:45:16.762019+00', '{"Latency": "-32394463", "SucceededAt": "1777290316757", "PerformanceDuration": "60"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4880, 1649, 'Processing', NULL, '2026-04-26 11:55:12.865766+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204512860"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4976, 1682, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:28:49.464204+00', '{"Queue": "default", "EnqueuedAt": "1777271329453"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5126, 1712, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:58:04.663171+00', '{"Queue": "default", "EnqueuedAt": "1777273084663"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5136, 1715, 'Processing', NULL, '2026-04-27 07:01:04.850222+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273264846"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5238, 1749, 'Processing', NULL, '2026-04-27 08:30:09.380367+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278609358"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5372, 1794, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 11:46:11.232563+00', '{"Queue": "default", "EnqueuedAt": "1777290371232"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5380, 1796, 'Succeeded', NULL, '2026-04-27 11:48:11.511852+00', '{"Latency": "-32399978", "SucceededAt": "1777290491506", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4883, 1650, 'Processing', NULL, '2026-04-26 11:56:12.937516+00', '{"ServerId": "desktop-o2bpcl9:2028:4cea8f8c-ef5d-4aca-9726-7a1541e7be32", "WorkerId": "99310b51-6577-45a6-99d4-a20e9775a882", "StartedAt": "1777204572931"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4977, 1682, 'Processing', NULL, '2026-04-27 06:28:49.510904+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271329493"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4990, 1683, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:29:35.024362+00', '{"FailedAt": "1777271375000", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4991, 1683, 'Scheduled', 'Retry attempt 2 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:29:35.026996+00', '{"EnqueueAt": "1777271393008", "ScheduledAt": "1777271375008"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5127, 1712, 'Processing', NULL, '2026-04-27 06:58:04.67362+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273084670"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5239, 1749, 'Succeeded', NULL, '2026-04-27 08:30:10.674664+00', '{"Latency": "-32399892", "SucceededAt": "1777278610658", "PerformanceDuration": "1271"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5245, 1751, 'Succeeded', NULL, '2026-04-27 08:32:09.621132+00', '{"Latency": "-32399979", "SucceededAt": "1777278729616", "PerformanceDuration": "63"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5373, 1794, 'Processing', NULL, '2026-04-27 11:46:16.991385+00', '{"ServerId": "desktop-o2bpcl9:24304:4983756f-ca44-41c0-9bee-33cb991a76c3", "WorkerId": "4943fb2e-7993-4287-86f4-7ecb5cfc171f", "StartedAt": "1777290376986"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4885, 1651, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:57:21.254218+00', '{"Queue": "default", "EnqueuedAt": "1777204641241"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4896, 1654, 'Succeeded', NULL, '2026-04-26 12:00:06.630065+00', '{"Latency": "-32399972", "SucceededAt": "1777204806625", "PerformanceDuration": "65"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4902, 1656, 'Succeeded', NULL, '2026-04-26 12:02:06.757911+00', '{"Latency": "-32399983", "SucceededAt": "1777204926753", "PerformanceDuration": "84"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4978, 1682, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:28:50.731336+00', '{"FailedAt": "1777271330702", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4979, 1682, 'Scheduled', 'Retry attempt 1 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:28:50.739243+00', '{"EnqueueAt": "1777271361713", "ScheduledAt": "1777271330713"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4995, 1683, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:30:04.757626+00', '{"FailedAt": "1777271404737", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4996, 1683, 'Scheduled', 'Retry attempt 3 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:30:04.760719+00', '{"EnqueueAt": "1777271495742", "ScheduledAt": "1777271404742"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5128, 1712, 'Succeeded', NULL, '2026-04-27 06:58:04.732299+00', '{"Latency": "-32399981", "SucceededAt": "1777273084726", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5130, 1713, 'Processing', NULL, '2026-04-27 06:59:04.730132+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273144726"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5132, 1714, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:00:04.781357+00', '{"Queue": "default", "EnqueuedAt": "1777273204781"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5240, 1750, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:31:09.457463+00', '{"Queue": "default", "EnqueuedAt": "1777278669456"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5250, 1753, 'Processing', NULL, '2026-04-27 08:34:09.708671+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278849705"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5374, 1794, 'Succeeded', NULL, '2026-04-27 11:46:17.103858+00', '{"Latency": "-32394229", "SucceededAt": "1777290377092", "PerformanceDuration": "93"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4886, 1651, 'Processing', NULL, '2026-04-26 11:57:21.291761+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204641283"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4980, 1683, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:29:04.520863+00', '{"Queue": "default", "EnqueuedAt": "1777271344520"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5129, 1713, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:59:04.720284+00', '{"Queue": "default", "EnqueuedAt": "1777273144720"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5140, 1716, 'Succeeded', NULL, '2026-04-27 07:02:04.98529+00', '{"Latency": "-32399980", "SucceededAt": "1777273324978", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4893, 1653, 'Succeeded', NULL, '2026-04-26 11:59:06.568258+00', '{"Latency": "-32399967", "SucceededAt": "1777204746563", "PerformanceDuration": "100"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5241, 1750, 'Processing', NULL, '2026-04-27 08:31:09.477104+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278669470"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5375, 1795, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 11:47:11.321461+00', '{"Queue": "default", "EnqueuedAt": "1777290431321"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4887, 1651, 'Succeeded', NULL, '2026-04-26 11:57:22.699148+00', '{"Latency": "-32399899", "SucceededAt": "1777204642666", "PerformanceDuration": "1364"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4981, 1683, 'Processing', NULL, '2026-04-27 06:29:04.535902+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271344531"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4993, 1683, 'Processing', NULL, '2026-04-27 06:30:04.501225+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271404497"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5005, 1684, 'Processing', NULL, '2026-04-27 06:30:49.592071+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271449587"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4901, 1656, 'Processing', NULL, '2026-04-26 12:02:06.665748+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204926662"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5013, 1682, 'Processing', NULL, '2026-04-27 06:31:19.635535+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271479631"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5131, 1713, 'Succeeded', NULL, '2026-04-27 06:59:04.787953+00', '{"Latency": "-32399983", "SucceededAt": "1777273144781", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5242, 1750, 'Succeeded', NULL, '2026-04-27 08:31:09.619301+00', '{"Latency": "-32399965", "SucceededAt": "1777278669609", "PerformanceDuration": "126"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5376, 1795, 'Processing', NULL, '2026-04-27 11:47:11.338551+00', '{"ServerId": "desktop-o2bpcl9:24304:4983756f-ca44-41c0-9bee-33cb991a76c3", "WorkerId": "07b20b19-c7e7-401b-98fe-6467060dda2e", "StartedAt": "1777290431333"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4888, 1652, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:58:06.358318+00', '{"Queue": "default", "EnqueuedAt": "1777204686358"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4982, 1683, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:29:04.835391+00', '{"FailedAt": "1777271344813", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4983, 1683, 'Scheduled', 'Retry attempt 1 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:29:04.838818+00', '{"EnqueueAt": "1777271366823", "ScheduledAt": "1777271344823"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4998, 1684, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:30:05.006813+00', '{"FailedAt": "1777271404988", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4999, 1684, 'Scheduled', 'Retry attempt 1 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:30:05.01037+00', '{"EnqueueAt": "1777271435994", "ScheduledAt": "1777271404994"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5133, 1714, 'Processing', NULL, '2026-04-27 07:00:04.792186+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273204788"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5137, 1715, 'Succeeded', NULL, '2026-04-27 07:01:04.911939+00', '{"Latency": "-32399981", "SucceededAt": "1777273264904", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5243, 1751, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:32:09.53646+00', '{"Queue": "default", "EnqueuedAt": "1777278729536"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5377, 1795, 'Succeeded', NULL, '2026-04-27 11:47:11.407546+00', '{"Latency": "-32399973", "SucceededAt": "1777290431400", "PerformanceDuration": "57"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4889, 1652, 'Processing', NULL, '2026-04-26 11:58:06.369751+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204686365"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4894, 1654, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:00:06.537624+00', '{"Queue": "default", "EnqueuedAt": "1777204806537"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4984, 1682, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:29:34.441742+00', '{"Queue": "default", "EnqueuedAt": "1777271374433"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4985, 1683, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:29:34.452926+00', '{"Queue": "default", "EnqueuedAt": "1777271374448"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4994, 1684, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:30:04.746312+00', '{"Queue": "default", "EnqueuedAt": "1777271404746"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5134, 1714, 'Succeeded', NULL, '2026-04-27 07:00:04.852751+00', '{"Latency": "-32399981", "SucceededAt": "1777273204846", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5135, 1715, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:01:04.838714+00', '{"Queue": "default", "EnqueuedAt": "1777273264838"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5246, 1752, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:33:09.617589+00', '{"Queue": "default", "EnqueuedAt": "1777278789617"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5378, 1796, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 11:48:11.434907+00', '{"Queue": "default", "EnqueuedAt": "1777290491434"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4986, 1682, 'Processing', NULL, '2026-04-27 06:29:34.452954+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271374449"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5000, 1682, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:30:19.530681+00', '{"Queue": "default", "EnqueuedAt": "1777271419524"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5138, 1716, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:02:04.914743+00', '{"Queue": "default", "EnqueuedAt": "1777273324914"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5247, 1752, 'Processing', NULL, '2026-04-27 08:33:09.62922+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278789625"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5379, 1796, 'Processing', NULL, '2026-04-27 11:48:11.447277+00', '{"ServerId": "desktop-o2bpcl9:24304:4983756f-ca44-41c0-9bee-33cb991a76c3", "WorkerId": "07b20b19-c7e7-401b-98fe-6467060dda2e", "StartedAt": "1777290491443"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4890, 1652, 'Succeeded', NULL, '2026-04-26 11:58:06.471252+00', '{"Latency": "-32399980", "SucceededAt": "1777204686462", "PerformanceDuration": "88"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4987, 1683, 'Processing', NULL, '2026-04-27 06:29:34.461123+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "d8bc2364-46d4-4f39-b4e0-3b7d1bef4461", "StartedAt": "1777271374457"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5010, 1685, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:31:05.124891+00', '{"FailedAt": "1777271465103", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5011, 1685, 'Scheduled', 'Retry attempt 1 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:31:05.127909+00', '{"EnqueueAt": "1777271503110", "ScheduledAt": "1777271465110"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4891, 1653, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 11:59:06.4369+00', '{"Queue": "default", "EnqueuedAt": "1777204746436"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5139, 1716, 'Processing', NULL, '2026-04-27 07:02:04.925892+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273324923"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5249, 1753, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:34:09.697538+00', '{"Queue": "default", "EnqueuedAt": "1777278849697"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5381, 1797, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 11:49:14.120348+00', '{"Queue": "default", "EnqueuedAt": "1777290554107"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4988, 1682, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:29:34.809517+00', '{"FailedAt": "1777271374773", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4989, 1682, 'Scheduled', 'Retry attempt 2 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:29:34.816818+00', '{"EnqueueAt": "1777271414793", "ScheduledAt": "1777271374793"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5141, 1717, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:03:05.002844+00', '{"Queue": "default", "EnqueuedAt": "1777273385002"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5146, 1718, 'Succeeded', NULL, '2026-04-27 07:04:05.173902+00', '{"Latency": "-32399982", "SucceededAt": "1777273445168", "PerformanceDuration": "45"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5148, 1719, 'Processing', NULL, '2026-04-27 07:05:05.184305+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273505180"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5153, 1721, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:07:05.357636+00', '{"Queue": "default", "EnqueuedAt": "1777273625357"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5251, 1753, 'Succeeded', NULL, '2026-04-27 08:34:09.773137+00', '{"Latency": "-32399980", "SucceededAt": "1777278849766", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5382, 1797, 'Processing', NULL, '2026-04-27 11:49:14.181385+00', '{"ServerId": "desktop-o2bpcl9:35540:d730349e-25da-41f3-8a12-86c9f4613787", "WorkerId": "16bd8e42-f1e5-49ca-aaf1-315b674484bf", "StartedAt": "1777290554156"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4992, 1683, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:30:04.488782+00', '{"Queue": "default", "EnqueuedAt": "1777271404484"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5009, 1685, 'Processing', NULL, '2026-04-27 06:31:04.85842+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271464854"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5142, 1717, 'Processing', NULL, '2026-04-27 07:03:05.015503+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273385011"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5144, 1718, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:04:05.107953+00', '{"Queue": "default", "EnqueuedAt": "1777273445107"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4892, 1653, 'Processing', NULL, '2026-04-26 11:59:06.456091+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204746450"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5252, 1754, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:35:09.77286+00', '{"Queue": "default", "EnqueuedAt": "1777278909772"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5266, 1758, 'Succeeded', NULL, '2026-04-27 08:39:10.200353+00', '{"Latency": "-32399967", "SucceededAt": "1777279150193", "PerformanceDuration": "63"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5383, 1797, 'Succeeded', NULL, '2026-04-27 11:49:14.790037+00', '{"Latency": "-32399877", "SucceededAt": "1777290554775", "PerformanceDuration": "586"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4997, 1684, 'Processing', NULL, '2026-04-27 06:30:04.760587+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "9cb5b89f-2155-43ab-9433-0f8bfe7eaca1", "StartedAt": "1777271404755"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5143, 1717, 'Succeeded', NULL, '2026-04-27 07:03:05.072887+00', '{"Latency": "-32399980", "SucceededAt": "1777273385067", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5149, 1719, 'Succeeded', NULL, '2026-04-27 07:05:05.238066+00', '{"Latency": "-32399983", "SucceededAt": "1777273505233", "PerformanceDuration": "46"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5152, 1720, 'Succeeded', NULL, '2026-04-27 07:06:05.331075+00', '{"Latency": "-32399983", "SucceededAt": "1777273565327", "PerformanceDuration": "43"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5253, 1754, 'Processing', NULL, '2026-04-27 08:35:09.785018+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278909781"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5001, 1682, 'Processing', NULL, '2026-04-27 06:30:19.542402+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271419536"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5145, 1718, 'Processing', NULL, '2026-04-27 07:04:05.119509+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "a8e25bc2-5a46-47fd-847e-a878276cc4b4", "StartedAt": "1777273445116"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4895, 1654, 'Processing', NULL, '2026-04-26 12:00:06.555144+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204806551"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5254, 1754, 'Succeeded', NULL, '2026-04-27 08:35:09.844163+00', '{"Latency": "-32399979", "SucceededAt": "1777278909837", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5002, 1682, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:30:19.849972+00', '{"FailedAt": "1777271419826", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5003, 1682, 'Scheduled', 'Retry attempt 3 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:30:19.854719+00', '{"EnqueueAt": "1777271471834", "ScheduledAt": "1777271419834"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5147, 1719, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:05:05.173492+00', '{"Queue": "default", "EnqueuedAt": "1777273505173"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5255, 1755, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:36:09.85905+00', '{"Queue": "default", "EnqueuedAt": "1777278969858"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5263, 1757, 'Succeeded', NULL, '2026-04-27 08:38:10.106921+00', '{"Latency": "-32399969", "SucceededAt": "1777279090098", "PerformanceDuration": "68"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4897, 1655, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:01:06.600012+00', '{"Queue": "default", "EnqueuedAt": "1777204866599"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5004, 1684, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:30:49.580818+00', '{"Queue": "default", "EnqueuedAt": "1777271449575"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5150, 1720, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:06:05.270176+00', '{"Queue": "default", "EnqueuedAt": "1777273565270"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4898, 1655, 'Processing', NULL, '2026-04-26 12:01:06.610955+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204866607"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5155, 1721, 'Succeeded', NULL, '2026-04-27 07:07:05.428532+00', '{"Latency": "-32399983", "SucceededAt": "1777273625422", "PerformanceDuration": "51"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5256, 1755, 'Processing', NULL, '2026-04-27 08:36:09.873361+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777278969868"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5006, 1684, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:30:49.843007+00', '{"FailedAt": "1777271449824", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5007, 1684, 'Scheduled', 'Retry attempt 2 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:30:49.847644+00', '{"EnqueueAt": "1777271499831", "ScheduledAt": "1777271449831"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5151, 1720, 'Processing', NULL, '2026-04-27 07:06:05.279549+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273565276"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5257, 1755, 'Succeeded', NULL, '2026-04-27 08:36:09.961256+00', '{"Latency": "-32399975", "SucceededAt": "1777278969949", "PerformanceDuration": "70"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5008, 1685, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:31:04.843444+00', '{"Queue": "default", "EnqueuedAt": "1777271464843"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5014, 1682, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:31:19.803849+00', '{"FailedAt": "1777271479786", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5015, 1682, 'Scheduled', 'Retry attempt 4 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:31:19.807168+00', '{"EnqueueAt": "1777271647791", "ScheduledAt": "1777271479791"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5154, 1721, 'Processing', NULL, '2026-04-27 07:07:05.367881+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273625364"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5258, 1756, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:37:09.930087+00', '{"Queue": "default", "EnqueuedAt": "1777279029929"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5265, 1758, 'Processing', NULL, '2026-04-27 08:39:10.124353+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279150118"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4899, 1655, 'Succeeded', NULL, '2026-04-26 12:01:06.703851+00', '{"Latency": "-32399981", "SucceededAt": "1777204866698", "PerformanceDuration": "82"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5012, 1682, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:31:19.625245+00', '{"Queue": "default", "EnqueuedAt": "1777271479621"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5156, 1722, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:08:05.447564+00', '{"Queue": "default", "EnqueuedAt": "1777273685447"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5162, 1724, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:10:05.609726+00', '{"Queue": "default", "EnqueuedAt": "1777273805609"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4900, 1656, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:02:06.655489+00', '{"Queue": "default", "EnqueuedAt": "1777204926655"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5164, 1724, 'Succeeded', NULL, '2026-04-27 07:10:05.666752+00', '{"Latency": "-32399983", "SucceededAt": "1777273805662", "PerformanceDuration": "40"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5259, 1756, 'Processing', NULL, '2026-04-27 08:37:09.940744+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279029937"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5016, 1683, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:31:49.664355+00', '{"Queue": "default", "EnqueuedAt": "1777271509660"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5017, 1684, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:31:49.671841+00', '{"Queue": "default", "EnqueuedAt": "1777271509667"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5019, 1685, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:31:49.68048+00', '{"Queue": "default", "EnqueuedAt": "1777271509675"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5157, 1722, 'Processing', NULL, '2026-04-27 07:08:05.458366+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273685454"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5174, 1728, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:14:05.909726+00', '{"Queue": "default", "EnqueuedAt": "1777274045909"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5177, 1729, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:15:05.988271+00', '{"Queue": "default", "EnqueuedAt": "1777274105988"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5260, 1756, 'Succeeded', NULL, '2026-04-27 08:37:10.021836+00', '{"Latency": "-32399981", "SucceededAt": "1777279030012", "PerformanceDuration": "68"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5018, 1683, 'Processing', NULL, '2026-04-27 06:31:49.672163+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271509668"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5030, 1686, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:32:05.128063+00', '{"FailedAt": "1777271525105", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5031, 1686, 'Scheduled', 'Retry attempt 1 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:32:05.130879+00', '{"EnqueueAt": "1777271568115", "ScheduledAt": "1777271525115"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5158, 1722, 'Succeeded', NULL, '2026-04-27 07:08:05.512187+00', '{"Latency": "-32399982", "SucceededAt": "1777273685506", "PerformanceDuration": "45"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5163, 1724, 'Processing', NULL, '2026-04-27 07:10:05.618529+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273805615"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5173, 1727, 'Succeeded', NULL, '2026-04-27 07:13:05.912632+00', '{"Latency": "-32399978", "SucceededAt": "1777273985907", "PerformanceDuration": "58"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5175, 1728, 'Processing', NULL, '2026-04-27 07:14:05.919543+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274045916"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5261, 1757, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:38:10.004264+00', '{"Queue": "default", "EnqueuedAt": "1777279090004"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5020, 1684, 'Processing', NULL, '2026-04-27 06:31:49.680882+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "9cb5b89f-2155-43ab-9433-0f8bfe7eaca1", "StartedAt": "1777271509676"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5159, 1723, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:09:05.536077+00', '{"Queue": "default", "EnqueuedAt": "1777273745535"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5262, 1757, 'Processing', NULL, '2026-04-27 08:38:10.023243+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279090017"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4903, 1657, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:03:06.741887+00', '{"Queue": "default", "EnqueuedAt": "1777204986741"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5021, 1685, 'Processing', NULL, '2026-04-27 06:31:49.689657+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "d8bc2364-46d4-4f39-b4e0-3b7d1bef4461", "StartedAt": "1777271509686"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5160, 1723, 'Processing', NULL, '2026-04-27 07:09:05.547656+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273745543"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5264, 1758, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:39:10.104371+00', '{"Queue": "default", "EnqueuedAt": "1777279150104"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5022, 1683, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:31:49.879379+00', '{"FailedAt": "1777271509858", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5023, 1683, 'Scheduled', 'Retry attempt 4 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:31:49.885074+00', '{"EnqueueAt": "1777271677865", "ScheduledAt": "1777271509865"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5029, 1686, 'Processing', NULL, '2026-04-27 06:32:04.928128+00', '{"ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "WorkerId": "6c77d81d-93ab-4531-94b6-7ea21723c28c", "StartedAt": "1777271524923"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5161, 1723, 'Succeeded', NULL, '2026-04-27 07:09:05.601103+00', '{"Latency": "-32399980", "SucceededAt": "1777273745595", "PerformanceDuration": "44"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5168, 1726, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:12:05.76053+00', '{"Queue": "default", "EnqueuedAt": "1777273925760"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5267, 1759, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:40:10.196113+00', '{"Queue": "default", "EnqueuedAt": "1777279210195"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5024, 1684, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:31:50.014196+00', '{"FailedAt": "1777271509993", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5025, 1684, 'Scheduled', 'Retry attempt 3 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:31:50.019379+00', '{"EnqueueAt": "1777271598000", "ScheduledAt": "1777271510000"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5165, 1725, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:11:05.684077+00', '{"Queue": "default", "EnqueuedAt": "1777273865683"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5166, 1725, 'Processing', NULL, '2026-04-27 07:11:05.794949+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "a8e25bc2-5a46-47fd-847e-a878276cc4b4", "StartedAt": "1777273865792"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5167, 1725, 'Succeeded', NULL, '2026-04-27 07:11:05.871354+00', '{"Latency": "-32399883", "SucceededAt": "1777273865864", "PerformanceDuration": "67"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5268, 1759, 'Processing', NULL, '2026-04-27 08:40:10.207895+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279210204"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5026, 1685, 'Failed', 'An exception occurred during performance of the job.', '2026-04-27 06:31:50.177513+00', '{"FailedAt": "1777271510158", "ServerId": "desktop-o2bpcl9:25860:306077f4-4edf-43da-81b6-060eada0f576", "ExceptionType": "Npgsql.PostgresException", "ExceptionDetails": "Npgsql.PostgresException: 23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null).\r\n   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\r\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\r\n   at Npgsql.NpgsqlCommand.ExecuteNonQuery(Boolean async, CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.ExecuteNonQueryDataAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteNonQueryAsync(CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.QueryRunner.NonQueryQueryAsync(Query query, IDataContext dataContext, Expression expression, Object[] ps, Object[] preambles, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.Linq.ExpressionQuery`1.LinqToDB.Async.IQueryProviderAsync.ExecuteAsync[TResult](Expression expression, CancellationToken cancellationToken)\r\n   at LinqToDB.LinqExtensions.UpdateAsync[T](IUpdatable`1 source, CancellationToken token)\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ProcessOrderInternal(Int32 orderId, Int32 branchId, Nullable`1 deliveryDate) in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 359\r\n   at TaskControl.TaskModule.Application.Services.OrderAssemblyPlannerJob.ExecuteAsync() in C:\\Учёба\\Диплом\\TaskControl\\TaskControl.TaskModule\\Application\\Services\\OrderAssemblyPlannerJob.cs:line 74\r\n   at InvokeStub_TaskAwaiter.GetResult(Object, Object, IntPtr*)\r\n   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)\r\n", "ExceptionMessage": "23514: new row for relation \"orders\" violates check constraint \"orders_status_check\"\r\n\r\nDETAIL: Failing row contains (2, 1, 1, 2026-04-27 07:21:17.783148, Pickup, Assembling, 2026-04-27 06:21:17.783148, null, Postpaid, null, null)."}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5027, 1685, 'Scheduled', 'Retry attempt 2 of 10: 23514: new row for relation "orders" violates che…', '2026-04-27 06:31:50.182054+00', '{"EnqueueAt": "1777271548164", "ScheduledAt": "1777271510164"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5169, 1726, 'Processing', NULL, '2026-04-27 07:12:05.770874+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273925768"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4904, 1657, 'Processing', NULL, '2026-04-26 12:03:06.753131+00', '{"ServerId": "desktop-o2bpcl9:4956:2e30b952-ded8-4fff-a072-edfade595aa3", "WorkerId": "b97ea4a9-4cbd-4f16-9bd9-ca6daaf3ba35", "StartedAt": "1777204986749"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5269, 1759, 'Succeeded', NULL, '2026-04-27 08:40:10.266199+00', '{"Latency": "-32399981", "SucceededAt": "1777279210259", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5272, 1760, 'Succeeded', NULL, '2026-04-27 08:41:10.336785+00', '{"Latency": "-32399981", "SucceededAt": "1777279270331", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5028, 1686, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:32:04.91052+00', '{"Queue": "default", "EnqueuedAt": "1777271524910"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5170, 1726, 'Succeeded', NULL, '2026-04-27 07:12:05.82547+00', '{"Latency": "-32399982", "SucceededAt": "1777273925820", "PerformanceDuration": "46"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5270, 1760, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:41:10.266866+00', '{"Queue": "default", "EnqueuedAt": "1777279270266"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4905, 1657, 'Succeeded', NULL, '2026-04-26 12:03:06.853228+00', '{"Latency": "-32399980", "SucceededAt": "1777204986844", "PerformanceDuration": "87"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5032, 1685, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:32:32.657098+00', '{"Queue": "default", "EnqueuedAt": "1777271552612"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5038, 1686, 'Processing', NULL, '2026-04-27 06:33:02.730127+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "a8e25bc2-5a46-47fd-847e-a878276cc4b4", "StartedAt": "1777271582726"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4906, 1658, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:12:51.529816+00', '{"Queue": "default", "EnqueuedAt": "1777205571513"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4919, 1662, 'Processing', NULL, '2026-04-26 12:16:07.048284+00', '{"ServerId": "desktop-o2bpcl9:23596:b9161b8a-73d4-4db9-852c-caae658c6755", "WorkerId": "158c2534-be96-4e11-bd0b-9786c30545b2", "StartedAt": "1777205767045"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5047, 1682, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:34:17.866956+00', '{"Queue": "default", "EnqueuedAt": "1777271657863"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5171, 1727, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:13:05.830999+00', '{"Queue": "default", "EnqueuedAt": "1777273985830"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5271, 1760, 'Processing', NULL, '2026-04-27 08:41:10.278467+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279270275"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5274, 1761, 'Processing', NULL, '2026-04-27 08:42:10.347515+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279330343"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5033, 1685, 'Processing', NULL, '2026-04-27 06:32:32.679756+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "7b6c9283-629b-4a4e-aca3-0c6cf8c01bc9", "StartedAt": "1777271552674"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4907, 1658, 'Processing', NULL, '2026-04-26 12:12:51.61219+00', '{"ServerId": "desktop-o2bpcl9:23596:b9161b8a-73d4-4db9-852c-caae658c6755", "WorkerId": "4f7f2aa3-b782-4af7-b6a9-9dd83ef0073a", "StartedAt": "1777205571598"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5061, 1691, 'Succeeded', NULL, '2026-04-27 06:37:03.094241+00', '{"Latency": "-32399980", "SucceededAt": "1777271823088", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5172, 1727, 'Processing', NULL, '2026-04-27 07:13:05.844897+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777273985841"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5273, 1761, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:42:10.3353+00', '{"Queue": "default", "EnqueuedAt": "1777279330335"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5034, 1685, 'Succeeded', NULL, '2026-04-27 06:32:33.610536+00', '{"Latency": "-32312152", "SucceededAt": "1777271553592", "PerformanceDuration": "906"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5059, 1691, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:37:03.02299+00', '{"Queue": "default", "EnqueuedAt": "1777271823022"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5176, 1728, 'Succeeded', NULL, '2026-04-27 07:14:05.999042+00', '{"Latency": "-32399983", "SucceededAt": "1777274045991", "PerformanceDuration": "68"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5178, 1729, 'Processing', NULL, '2026-04-27 07:15:05.998911+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274105995"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5275, 1761, 'Succeeded', NULL, '2026-04-27 08:42:10.41235+00', '{"Latency": "-32399980", "SucceededAt": "1777279330406", "PerformanceDuration": "56"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5035, 1687, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:33:02.69049+00', '{"Queue": "default", "EnqueuedAt": "1777271582689"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5056, 1690, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:36:02.949857+00', '{"Queue": "default", "EnqueuedAt": "1777271762949"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5179, 1729, 'Succeeded', NULL, '2026-04-27 07:15:06.064762+00', '{"Latency": "-32399984", "SucceededAt": "1777274106058", "PerformanceDuration": "57"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5276, 1762, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:43:10.403721+00', '{"Queue": "default", "EnqueuedAt": "1777279390403"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5036, 1687, 'Processing', NULL, '2026-04-27 06:33:02.70653+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271582702"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5180, 1730, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:16:06.080124+00', '{"Queue": "default", "EnqueuedAt": "1777274166080"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5277, 1762, 'Processing', NULL, '2026-04-27 08:43:10.41702+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279390413"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5283, 1764, 'Processing', NULL, '2026-04-27 08:45:10.57533+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279510572"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5287, 1765, 'Succeeded', NULL, '2026-04-27 08:46:10.712611+00', '{"Latency": "-32399982", "SucceededAt": "1777279570707", "PerformanceDuration": "45"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4908, 1658, 'Succeeded', NULL, '2026-04-26 12:12:53.007626+00', '{"Latency": "-32399799", "SucceededAt": "1777205572993", "PerformanceDuration": "1355"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4915, 1661, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:15:06.947941+00', '{"Queue": "default", "EnqueuedAt": "1777205706947"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5037, 1686, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:33:02.719672+00', '{"Queue": "default", "EnqueuedAt": "1777271582714"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5044, 1688, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:34:02.799313+00', '{"Queue": "default", "EnqueuedAt": "1777271642799"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5181, 1730, 'Processing', NULL, '2026-04-27 07:16:06.095066+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274166089"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5278, 1762, 'Succeeded', NULL, '2026-04-27 08:43:10.472509+00', '{"Latency": "-32399980", "SucceededAt": "1777279390468", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5039, 1687, 'Succeeded', NULL, '2026-04-27 06:33:02.780711+00', '{"Latency": "-32399963", "SucceededAt": "1777271582772", "PerformanceDuration": "62"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4909, 1659, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:13:06.659732+00', '{"Queue": "default", "EnqueuedAt": "1777205586659"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5057, 1690, 'Processing', NULL, '2026-04-27 06:36:02.959542+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271762956"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5182, 1730, 'Succeeded', NULL, '2026-04-27 07:16:06.179343+00', '{"Latency": "-32399975", "SucceededAt": "1777274166172", "PerformanceDuration": "71"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5279, 1763, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:44:10.477349+00', '{"Queue": "default", "EnqueuedAt": "1777279450477"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5040, 1686, 'Succeeded', NULL, '2026-04-27 06:33:02.807084+00', '{"Latency": "-32342171", "SucceededAt": "1777271582800", "PerformanceDuration": "65"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4910, 1659, 'Processing', NULL, '2026-04-26 12:13:06.676919+00', '{"ServerId": "desktop-o2bpcl9:23596:b9161b8a-73d4-4db9-852c-caae658c6755", "WorkerId": "158c2534-be96-4e11-bd0b-9786c30545b2", "StartedAt": "1777205586671"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4921, 1663, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:17:07.111411+00', '{"Queue": "default", "EnqueuedAt": "1777205827111"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5041, 1684, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:33:32.790257+00', '{"Queue": "default", "EnqueuedAt": "1777271612786"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5058, 1690, 'Succeeded', NULL, '2026-04-27 06:36:03.020482+00', '{"Latency": "-32399981", "SucceededAt": "1777271763014", "PerformanceDuration": "51"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5183, 1731, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:17:06.19523+00', '{"Queue": "default", "EnqueuedAt": "1777274226195"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5280, 1763, 'Processing', NULL, '2026-04-27 08:44:10.487862+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279450484"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5042, 1684, 'Processing', NULL, '2026-04-27 06:33:32.799778+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271612795"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5055, 1689, 'Succeeded', NULL, '2026-04-27 06:35:02.970165+00', '{"Latency": "-32399977", "SucceededAt": "1777271702953", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5184, 1731, 'Processing', NULL, '2026-04-27 07:17:06.205266+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274226201"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5281, 1763, 'Succeeded', NULL, '2026-04-27 08:44:10.551016+00', '{"Latency": "-32399982", "SucceededAt": "1777279450544", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5043, 1684, 'Succeeded', NULL, '2026-04-27 06:33:32.872132+00', '{"Latency": "-32191937", "SucceededAt": "1777271612866", "PerformanceDuration": "61"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5049, 1682, 'Succeeded', NULL, '2026-04-27 06:34:17.935238+00', '{"Latency": "-32071535", "SucceededAt": "1777271657930", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5185, 1731, 'Succeeded', NULL, '2026-04-27 07:17:06.266995+00', '{"Latency": "-32399984", "SucceededAt": "1777274226262", "PerformanceDuration": "54"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5282, 1764, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:45:10.56506+00', '{"Queue": "default", "EnqueuedAt": "1777279510564"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5290, 1766, 'Succeeded', NULL, '2026-04-27 08:47:10.771261+00', '{"Latency": "-32399983", "SucceededAt": "1777279630766", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5045, 1688, 'Processing', NULL, '2026-04-27 06:34:02.810947+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271642806"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5186, 1732, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:18:06.271609+00', '{"Queue": "default", "EnqueuedAt": "1777274286271"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5284, 1764, 'Succeeded', NULL, '2026-04-27 08:45:10.636122+00', '{"Latency": "-32399982", "SucceededAt": "1777279510629", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4911, 1659, 'Succeeded', NULL, '2026-04-26 12:13:06.779169+00', '{"Latency": "-32399972", "SucceededAt": "1777205586769", "PerformanceDuration": "87"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4920, 1662, 'Succeeded', NULL, '2026-04-26 12:16:07.116311+00', '{"Latency": "-32399983", "SucceededAt": "1777205767111", "PerformanceDuration": "59"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5294, 1768, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:49:10.856313+00', '{"Queue": "default", "EnqueuedAt": "1777279750856"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5301, 1770, 'Processing', NULL, '2026-04-27 08:51:11.052678+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279871049"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5046, 1688, 'Succeeded', NULL, '2026-04-27 06:34:02.883694+00', '{"Latency": "-32399978", "SucceededAt": "1777271642876", "PerformanceDuration": "59"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5187, 1732, 'Processing', NULL, '2026-04-27 07:18:06.282987+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274286279"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5285, 1765, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:46:10.648496+00', '{"Queue": "default", "EnqueuedAt": "1777279570648"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5292, 1767, 'Processing', NULL, '2026-04-27 08:48:10.797152+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279690793"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4912, 1660, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:14:06.810703+00', '{"Queue": "default", "EnqueuedAt": "1777205646810"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4918, 1662, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:16:07.038182+00', '{"Queue": "default", "EnqueuedAt": "1777205767038"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5048, 1682, 'Processing', NULL, '2026-04-27 06:34:17.874615+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271657870"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4913, 1660, 'Processing', NULL, '2026-04-26 12:14:06.82504+00', '{"ServerId": "desktop-o2bpcl9:23596:b9161b8a-73d4-4db9-852c-caae658c6755", "WorkerId": "158c2534-be96-4e11-bd0b-9786c30545b2", "StartedAt": "1777205646820"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4916, 1661, 'Processing', NULL, '2026-04-26 12:15:06.959032+00', '{"ServerId": "desktop-o2bpcl9:23596:b9161b8a-73d4-4db9-852c-caae658c6755", "WorkerId": "158c2534-be96-4e11-bd0b-9786c30545b2", "StartedAt": "1777205706955"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5188, 1732, 'Succeeded', NULL, '2026-04-27 07:18:06.338532+00', '{"Latency": "-32399982", "SucceededAt": "1777274286332", "PerformanceDuration": "46"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5286, 1765, 'Processing', NULL, '2026-04-27 08:46:10.659405+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279570655"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5050, 1683, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-04-27 06:34:47.904997+00', '{"Queue": "default", "EnqueuedAt": "1777271687901"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5189, 1733, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:19:06.3334+00', '{"Queue": "default", "EnqueuedAt": "1777274346333"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5288, 1766, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:47:10.704564+00', '{"Queue": "default", "EnqueuedAt": "1777279630704"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5296, 1768, 'Succeeded', NULL, '2026-04-27 08:49:10.937429+00', '{"Latency": "-32399975", "SucceededAt": "1777279750931", "PerformanceDuration": "54"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5306, 1772, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:53:11.178545+00', '{"Queue": "default", "EnqueuedAt": "1777279991178"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4914, 1660, 'Succeeded', NULL, '2026-04-26 12:14:06.899371+00', '{"Latency": "-32399976", "SucceededAt": "1777205646893", "PerformanceDuration": "64"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5051, 1683, 'Processing', NULL, '2026-04-27 06:34:47.916127+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271687909"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5190, 1733, 'Processing', NULL, '2026-04-27 07:19:06.343405+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274346339"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5289, 1766, 'Processing', NULL, '2026-04-27 08:47:10.71517+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279630712"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5302, 1770, 'Succeeded', NULL, '2026-04-27 08:51:11.117853+00', '{"Latency": "-32399982", "SucceededAt": "1777279871113", "PerformanceDuration": "57"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4917, 1661, 'Succeeded', NULL, '2026-04-26 12:15:07.03204+00', '{"Latency": "-32399981", "SucceededAt": "1777205707026", "PerformanceDuration": "63"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5052, 1683, 'Succeeded', NULL, '2026-04-27 06:34:47.976687+00', '{"Latency": "-32056595", "SucceededAt": "1777271687971", "PerformanceDuration": "51"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5054, 1689, 'Processing', NULL, '2026-04-27 06:35:02.895461+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271702891"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5060, 1691, 'Processing', NULL, '2026-04-27 06:37:03.035575+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271823031"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5191, 1733, 'Succeeded', NULL, '2026-04-27 07:19:06.399645+00', '{"Latency": "-32399984", "SucceededAt": "1777274346394", "PerformanceDuration": "48"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5192, 1734, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:20:06.400605+00', '{"Queue": "default", "EnqueuedAt": "1777274406400"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5291, 1767, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:48:10.786548+00', '{"Queue": "default", "EnqueuedAt": "1777279690786"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5053, 1689, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:35:02.883459+00', '{"Queue": "default", "EnqueuedAt": "1777271702883"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5193, 1734, 'Processing', NULL, '2026-04-27 07:20:06.411314+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777274406407"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5293, 1767, 'Succeeded', NULL, '2026-04-27 08:48:10.863452+00', '{"Latency": "-32399982", "SucceededAt": "1777279690858", "PerformanceDuration": "58"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5303, 1771, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:52:11.111801+00', '{"Queue": "default", "EnqueuedAt": "1777279931111"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5307, 1772, 'Processing', NULL, '2026-04-27 08:53:11.188762+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279991185"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4922, 1663, 'Processing', NULL, '2026-04-26 12:17:07.122041+00', '{"ServerId": "desktop-o2bpcl9:23596:b9161b8a-73d4-4db9-852c-caae658c6755", "WorkerId": "158c2534-be96-4e11-bd0b-9786c30545b2", "StartedAt": "1777205827118"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5062, 1692, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:38:03.112905+00', '{"Queue": "default", "EnqueuedAt": "1777271883112"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5194, 1734, 'Succeeded', NULL, '2026-04-27 07:20:06.488237+00', '{"Latency": "-32399983", "SucceededAt": "1777274406481", "PerformanceDuration": "67"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5295, 1768, 'Processing', NULL, '2026-04-27 08:49:10.872162+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279750867"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4923, 1663, 'Succeeded', NULL, '2026-04-26 12:17:07.193347+00', '{"Latency": "-32399980", "SucceededAt": "1777205827187", "PerformanceDuration": "60"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5063, 1692, 'Processing', NULL, '2026-04-27 06:38:03.12464+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271883120"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5065, 1693, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:39:03.184348+00', '{"Queue": "default", "EnqueuedAt": "1777271943184"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5195, 1735, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:46:06.253354+00', '{"Queue": "default", "EnqueuedAt": "1777275966242"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5297, 1769, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:50:10.962958+00', '{"Queue": "default", "EnqueuedAt": "1777279810962"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5298, 1769, 'Processing', NULL, '2026-04-27 08:50:11.102591+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279811099"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5299, 1769, 'Succeeded', NULL, '2026-04-27 08:50:11.158461+00', '{"Latency": "-32399853", "SucceededAt": "1777279811153", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5310, 1773, 'Processing', NULL, '2026-04-27 08:54:11.243606+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280051240"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5064, 1692, 'Succeeded', NULL, '2026-04-27 06:38:03.186277+00', '{"Latency": "-32399980", "SucceededAt": "1777271883179", "PerformanceDuration": "51"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4924, 1664, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:32:56.924477+00', '{"Queue": "default", "EnqueuedAt": "1777206776911"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5196, 1735, 'Processing', NULL, '2026-04-27 07:46:07.326724+00', '{"ServerId": "desktop-o2bpcl9:12372:d1af9ed2-d9fd-4b09-981d-f2f330e2a4b0", "WorkerId": "c6d9e90a-ef38-45c1-a628-e2db41c69eb2", "StartedAt": "1777275967319"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5300, 1770, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:51:11.042278+00', '{"Queue": "default", "EnqueuedAt": "1777279871042"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5304, 1771, 'Processing', NULL, '2026-04-27 08:52:11.122207+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777279931119"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5066, 1693, 'Processing', NULL, '2026-04-27 06:39:03.196888+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777271943191"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5197, 1735, 'Succeeded', NULL, '2026-04-27 07:46:08.427266+00', '{"Latency": "-32398834", "SucceededAt": "1777275968413", "PerformanceDuration": "1076"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4925, 1664, 'Processing', NULL, '2026-04-26 12:32:56.961154+00', '{"ServerId": "desktop-o2bpcl9:38088:aafe1fe8-a66f-4ef8-a146-b9aac401b94a", "WorkerId": "2bb969a4-8b53-4992-836a-256abe55a78c", "StartedAt": "1777206776954"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4932, 1666, 'Succeeded', NULL, '2026-04-26 12:34:12.993562+00', '{"Latency": "-32399977", "SucceededAt": "1777206852988", "PerformanceDuration": "90"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5305, 1771, 'Succeeded', NULL, '2026-04-27 08:52:11.175355+00', '{"Latency": "-32399983", "SucceededAt": "1777279931169", "PerformanceDuration": "44"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5308, 1772, 'Succeeded', NULL, '2026-04-27 08:53:11.244096+00', '{"Latency": "-32399983", "SucceededAt": "1777279991239", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5067, 1693, 'Succeeded', NULL, '2026-04-27 06:39:03.25877+00', '{"Latency": "-32399980", "SucceededAt": "1777271943252", "PerformanceDuration": "52"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5075, 1696, 'Processing', NULL, '2026-04-27 06:42:03.456537+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "b6ba7dc6-ff17-45d6-9a1d-26a5f3bd4a7b", "StartedAt": "1777272123453"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5198, 1736, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:47:06.305776+00', '{"Queue": "default", "EnqueuedAt": "1777276026299"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5309, 1773, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:54:11.232856+00', '{"Queue": "default", "EnqueuedAt": "1777280051232"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5068, 1694, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:40:03.293565+00', '{"Queue": "default", "EnqueuedAt": "1777272003293"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5069, 1694, 'Processing', NULL, '2026-04-27 06:40:03.327569+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "7b6c9283-629b-4a4e-aca3-0c6cf8c01bc9", "StartedAt": "1777272003323"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5070, 1694, 'Succeeded', NULL, '2026-04-27 06:40:03.39785+00', '{"Latency": "-32399953", "SucceededAt": "1777272003390", "PerformanceDuration": "59"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5199, 1736, 'Processing', NULL, '2026-04-27 07:47:16.326032+00', '{"ServerId": "desktop-o2bpcl9:1488:252fda07-887a-4f47-95e2-16469e0c0fc6", "WorkerId": "de4d5d11-85e5-4339-841c-54965966180a", "StartedAt": "1777276036307"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5311, 1773, 'Succeeded', NULL, '2026-04-27 08:54:11.305158+00', '{"Latency": "-32399981", "SucceededAt": "1777280051298", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4926, 1664, 'Succeeded', NULL, '2026-04-26 12:32:58.34854+00', '{"Latency": "-32399901", "SucceededAt": "1777206778330", "PerformanceDuration": "1360"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5071, 1695, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:41:03.375879+00', '{"Queue": "default", "EnqueuedAt": "1777272063375"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5072, 1695, 'Processing', NULL, '2026-04-27 06:41:03.433675+00', '{"ServerId": "desktop-o2bpcl9:2692:6da19780-494b-49ec-97b3-87948012a4b2", "WorkerId": "a8e25bc2-5a46-47fd-847e-a878276cc4b4", "StartedAt": "1777272063430"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5073, 1695, 'Succeeded', NULL, '2026-04-27 06:41:03.488107+00', '{"Latency": "-32399936", "SucceededAt": "1777272063483", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5200, 1736, 'Succeeded', NULL, '2026-04-27 07:47:17.397583+00', '{"Latency": "-32389938", "SucceededAt": "1777276037382", "PerformanceDuration": "1049"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5312, 1774, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:55:11.305476+00', '{"Queue": "default", "EnqueuedAt": "1777280111305"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5315, 1775, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:56:11.405123+00', '{"Queue": "default", "EnqueuedAt": "1777280171404"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5320, 1776, 'Succeeded', NULL, '2026-04-27 08:57:11.532173+00', '{"Latency": "-32399981", "SucceededAt": "1777280231527", "PerformanceDuration": "47"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5323, 1777, 'Succeeded', NULL, '2026-04-27 08:58:11.5853+00', '{"Latency": "-32399984", "SucceededAt": "1777280291581", "PerformanceDuration": "43"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5324, 1778, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:59:11.616349+00', '{"Queue": "default", "EnqueuedAt": "1777280351616"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5074, 1696, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:42:03.443786+00', '{"Queue": "default", "EnqueuedAt": "1777272123443"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5201, 1737, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 07:48:05.020983+00', '{"Queue": "default", "EnqueuedAt": "1777276085011"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5313, 1774, 'Processing', NULL, '2026-04-27 08:55:11.319554+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280111315"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5314, 1774, 'Succeeded', NULL, '2026-04-27 08:55:11.387783+00', '{"Latency": "-32399977", "SucceededAt": "1777280111380", "PerformanceDuration": "56"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5318, 1776, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:57:11.464742+00', '{"Queue": "default", "EnqueuedAt": "1777280231464"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5321, 1777, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 08:58:11.524869+00', '{"Queue": "default", "EnqueuedAt": "1777280291524"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5076, 1696, 'Succeeded', NULL, '2026-04-27 06:42:03.516772+00', '{"Latency": "-32399978", "SucceededAt": "1777272123511", "PerformanceDuration": "50"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4927, 1665, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-26 12:33:11.996409+00', '{"Queue": "default", "EnqueuedAt": "1777206791996"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4931, 1666, 'Processing', NULL, '2026-04-26 12:34:12.893157+00', '{"ServerId": "desktop-o2bpcl9:38088:aafe1fe8-a66f-4ef8-a146-b9aac401b94a", "WorkerId": "2bb969a4-8b53-4992-836a-256abe55a78c", "StartedAt": "1777206852889"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5202, 1737, 'Processing', NULL, '2026-04-27 07:48:05.046379+00', '{"ServerId": "desktop-o2bpcl9:14740:2b039c95-450f-4c00-bc50-a29d0039c747", "WorkerId": "9a3f07ca-df24-416b-9b6c-4c86ec096410", "StartedAt": "1777276085043"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5316, 1775, 'Processing', NULL, '2026-04-27 08:56:11.416775+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280171413"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5319, 1776, 'Processing', NULL, '2026-04-27 08:57:11.475955+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280231472"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5327, 1779, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 09:00:11.678197+00', '{"Queue": "default", "EnqueuedAt": "1777280411678"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5331, 1780, 'Processing', NULL, '2026-04-27 09:01:11.745142+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280471742"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5077, 1697, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:43:03.507702+00', '{"Queue": "default", "EnqueuedAt": "1777272183507"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5080, 1698, 'Enqueued', 'Triggered by recurring job scheduler', '2026-04-27 06:44:03.572213+00', '{"Queue": "default", "EnqueuedAt": "1777272243572"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5203, 1737, 'Succeeded', NULL, '2026-04-27 07:48:06.090496+00', '{"Latency": "-32399932", "SucceededAt": "1777276086077", "PerformanceDuration": "1026"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5317, 1775, 'Succeeded', NULL, '2026-04-27 08:56:11.479201+00', '{"Latency": "-32399981", "SucceededAt": "1777280171473", "PerformanceDuration": "53"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (5325, 1778, 'Processing', NULL, '2026-04-27 08:59:11.627907+00', '{"ServerId": "desktop-o2bpcl9:24720:4ec3951a-abc2-4c53-9136-86f4e13138c7", "WorkerId": "28732235-eae8-4c6c-a79f-5fecdf575024", "StartedAt": "1777280351623"}', 0);
INSERT INTO hangfire.state (id, jobid, name, reason, createdat, data, updatecount) VALUES (4928, 1665, 'Processing', NULL, '2026-04-26 12:33:12.015681+00', '{"ServerId": "desktop-o2bpcl9:38088:aafe1fe8-a66f-4ef8-a146-b9aac401b94a", "WorkerId": "2bb969a4-8b53-4992-836a-256abe55a78c", "StartedAt": "1777206792009"}', 0);


--
-- TOC entry 3948 (class 0 OID 16483)
-- Dependencies: 228
-- Data for Name: active_assigned_tasks; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3946 (class 0 OID 16458)
-- Dependencies: 226
-- Data for Name: base_tasks; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.base_tasks (task_id, title, description, branch_id, type, created_at, completed_at, status, deadline, priority_level, source_type) VALUES (15, 'Сборка заказа 2', 'Автоматически спланированная сборка заказа', 1, 'OrderAssembly', '2026-04-27 06:32:33.415851', NULL, 'New', '2026-04-27 07:21:17.783148', 2, 'Server');


--
-- TOC entry 3938 (class 0 OID 16396)
-- Dependencies: 218
-- Data for Name: branches; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.branches (branch_id, branch_name, branch_type, address, created_at) VALUES (1, 'Центральный склад', 'Warehouse', 'г. Москва, ул. Ленина, 1', '2026-04-12 07:28:35.281321');
INSERT INTO public.branches (branch_id, branch_name, branch_type, address, created_at) VALUES (2, 'Филиал Восток', 'Retail', 'г. Владивосток, ул. Портовая, 42', '2026-04-12 07:28:35.281321');
INSERT INTO public.branches (branch_id, branch_name, branch_type, address, created_at) VALUES (3, 'Филиал Запад', 'Distribution', 'г. Калининград, пр. Мира, 15', '2026-04-12 07:28:35.281321');


--
-- TOC entry 3942 (class 0 OID 16421)
-- Dependencies: 222
-- Data for Name: check_io_employees; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (1, 1, 1, 'in', '2025-07-16 08:00:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (2, 1, 1, 'out', '2025-07-16 17:30:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (3, 2, 2, 'in', '2025-07-16 09:15:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (4, 2, 2, 'out', '2025-07-16 18:30:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (5, 3, 1, 'in', '2025-07-16 07:45:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (6, 3, 1, 'out', '2025-07-16 16:20:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (7, 1, 1, 'in', '2025-07-17 08:05:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (8, 1, 1, 'out', '2025-07-17 17:40:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (9, 2, 2, 'in', '2025-07-17 09:20:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (10, 2, 2, 'out', '2025-07-17 18:15:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (11, 3, 1, 'in', '2025-07-17 07:50:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (12, 3, 1, 'out', '2025-07-17 16:35:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (13, 1, 1, 'in', '2025-07-18 08:10:00');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (14, 1, 1, 'in', '2026-04-12 05:28:35.34321');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (15, 2, 1, 'in', '2026-04-12 06:28:35.34321');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (16, 3, 1, 'in', '2026-04-12 06:58:35.34321');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (17, 1, 1, 'in', '2026-04-14 03:22:40.783657');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (18, 2, 1, 'in', '2026-04-14 04:22:40.783657');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (19, 3, 1, 'in', '2026-04-14 04:52:40.783657');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (20, 1, 1, 'in', '2026-04-17 01:35:46.111076');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (21, 2, 1, 'in', '2026-04-17 02:35:46.111076');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (22, 3, 1, 'in', '2026-04-17 03:05:46.111076');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (23, 1, 1, 'in', '2026-04-19 08:50:36.927477');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (24, 2, 1, 'in', '2026-04-19 09:50:36.927477');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (25, 3, 1, 'in', '2026-04-19 10:20:36.927477');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (26, 1, 1, 'in', '2026-04-19 08:59:27.937206');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (27, 2, 1, 'in', '2026-04-19 09:59:27.937206');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (28, 3, 1, 'in', '2026-04-19 10:29:27.937206');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (29, 1, 1, 'in', '2026-04-20 08:02:20.004368');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (30, 2, 1, 'in', '2026-04-20 09:02:20.004368');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (31, 3, 1, 'in', '2026-04-20 09:32:20.004368');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (32, 1, 1, 'in', '2026-04-21 03:41:44.567249');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (33, 2, 1, 'in', '2026-04-21 04:41:44.567249');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (34, 3, 1, 'in', '2026-04-21 05:11:44.567249');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (35, 1, 1, 'in', '2026-04-24 07:42:24.729579');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (36, 2, 1, 'in', '2026-04-24 08:42:24.729579');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (37, 3, 1, 'in', '2026-04-24 09:12:24.729579');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (38, 1, 1, 'in', '2026-04-24 22:11:41.35937');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (39, 2, 1, 'in', '2026-04-24 23:11:41.35937');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (40, 3, 1, 'in', '2026-04-24 23:41:41.35937');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (41, 1, 1, 'in', '2026-04-24 22:23:04.811109');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (42, 2, 1, 'in', '2026-04-24 23:23:04.811109');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (43, 3, 1, 'in', '2026-04-24 23:53:04.811109');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (44, 1, 1, 'in', '2026-04-24 22:24:25.163875');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (45, 2, 1, 'in', '2026-04-24 23:24:25.163875');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (46, 3, 1, 'in', '2026-04-24 23:54:25.163875');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (47, 1, 1, 'in', '2026-04-25 03:55:24.381166');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (48, 2, 1, 'in', '2026-04-25 04:55:24.381166');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (49, 3, 1, 'in', '2026-04-25 05:25:24.381166');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (50, 1, 1, 'in', '2026-04-26 09:54:11.227876');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (51, 2, 1, 'in', '2026-04-26 10:54:11.227876');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (52, 3, 1, 'in', '2026-04-26 11:24:11.227876');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (53, 1, 1, 'in', '2026-04-27 04:20:57.825349');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (54, 2, 1, 'in', '2026-04-27 05:20:57.825349');
INSERT INTO public.check_io_employees (id, employee_id, branch_id, check_type, check_timestamp) VALUES (55, 3, 1, 'in', '2026-04-27 05:50:57.825349');


--
-- TOC entry 4004 (class 0 OID 52089)
-- Dependencies: 284
-- Data for Name: courier_capabilities; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 4006 (class 0 OID 52118)
-- Dependencies: 286
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3936 (class 0 OID 16386)
-- Dependencies: 216
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.employees (employees_id, surname, name, middle_name, created_at, role_id) VALUES (1, 'Иванов', 'Иван', 'Иванович', '2026-04-12 07:28:35.282087', 1);
INSERT INTO public.employees (employees_id, surname, name, middle_name, created_at, role_id) VALUES (2, 'Петрова', 'Мария', 'Сергеевна', '2026-04-12 07:28:35.282087', 1);
INSERT INTO public.employees (employees_id, surname, name, middle_name, created_at, role_id) VALUES (3, 'Сидоров', 'Алексей', NULL, '2026-04-12 07:28:35.282087', 1);
INSERT INTO public.employees (employees_id, surname, name, middle_name, created_at, role_id) VALUES (4, 'Ошлаков', 'Данил', 'Сергеевич', '2026-04-17 03:29:52.183992', 3);


--
-- TOC entry 3966 (class 0 OID 16687)
-- Dependencies: 246
-- Data for Name: inventory_assignment_lines; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3964 (class 0 OID 16659)
-- Dependencies: 244
-- Data for Name: inventory_assignments; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3968 (class 0 OID 16714)
-- Dependencies: 248
-- Data for Name: inventory_discrepancies; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3970 (class 0 OID 16744)
-- Dependencies: 250
-- Data for Name: inventory_statistics; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3960 (class 0 OID 16608)
-- Dependencies: 240
-- Data for Name: item_movements; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3956 (class 0 OID 16565)
-- Dependencies: 236
-- Data for Name: item_positions; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.item_positions (id, item_id, position_id, quantity, created_at) VALUES (1, 3, 1, 10, '2026-04-25 16:08:16.148282');


--
-- TOC entry 3962 (class 0 OID 16641)
-- Dependencies: 242
-- Data for Name: item_statuses; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--



--
-- TOC entry 3940 (class 0 OID 16408)
-- Dependencies: 220
-- Data for Name: items; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.items (item_id, name, weight, length, width, height, created_at) VALUES (1, 'Телефон No Kia', 1.5, 20, 15, 10, '2026-04-12 07:28:35.283102');
INSERT INTO public.items (item_id, name, weight, length, width, height, created_at) VALUES (2, 'Плашка ОЗУ', 0.8, 10, 10, 5, '2026-04-12 07:28:35.283102');
INSERT INTO public.items (item_id, name, weight, length, width, height, created_at) VALUES (3, 'Видеокарта ХХХ6090', 5, 50, 30, 20, '2026-04-12 07:28:35.283102');


--
-- TOC entry 3976 (class 0 OID 16837)
-- Dependencies: 256
-- Data for Name: mobile_app_users; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.mobile_app_users (id, employee_id, password_hash, role, is_active, created_at, updated_at, branch_id) VALUES (1, 1, 'a4ayc/80/OGda4BO/1o/V0etpOqiLx1JwB5S3beHW0s=', 1, true, '2026-04-14 05:55:50.073735', NULL, 1);
INSERT INTO public.mobile_app_users (id, employee_id, password_hash, role, is_active, created_at, updated_at, branch_id) VALUES (3, 4, 'a4ayc/80/OGda4BO/1o/V0etpOqiLx1JwB5S3beHW0s=', 2, true, '2026-04-17 03:29:58.532817', NULL, 1);


--
-- TOC entry 3972 (class 0 OID 16769)
-- Dependencies: 252
-- Data for Name: order_assembly_assignments; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.order_assembly_assignments (id, task_id, order_id, assigned_to_user_id, branch_id, status, assigned_at, completed_at, started_at) VALUES (15, 15, 2, 1, 1, 0, '2026-04-27 06:32:33.523199', NULL, NULL);


--
-- TOC entry 3974 (class 0 OID 16803)
-- Dependencies: 254
-- Data for Name: order_assembly_lines; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.order_assembly_lines (id, order_assembly_assignment_id, item_position_id, source_position_id, target_position_id, quantity, status, picked_quantity) VALUES (29, 15, 1, 1, 70, 2, 0, 0);
INSERT INTO public.order_assembly_lines (id, order_assembly_assignment_id, item_position_id, source_position_id, target_position_id, quantity, status, picked_quantity) VALUES (30, 15, 1, 1, 68, 1, 0, 0);


--
-- TOC entry 3954 (class 0 OID 16544)
-- Dependencies: 234
-- Data for Name: order_positions; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (1, 1, 1, 2, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (2, 1, 2, 1, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (3, 2, 3, 3, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (4, 3, 1, 3, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (5, 3, 3, 1, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (6, 4, 2, 4, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_positions (unique_id, order_id, item_id, quantity, created_at) VALUES (7, 5, 1, 2, '2026-04-27 06:21:17.783148');


--
-- TOC entry 3958 (class 0 OID 16587)
-- Dependencies: 238
-- Data for Name: order_reservations; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (4, 4, NULL, 3, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (5, 5, NULL, 1, '2026-04-27 06:21:17.783148');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (734, 1, NULL, 2, '2026-04-27 11:49:14.703498');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (735, 1, NULL, 2, '2026-04-27 11:49:14.727115');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (736, 2, NULL, 1, '2026-04-27 11:49:14.73072');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (737, 2, NULL, 1, '2026-04-27 11:49:14.732224');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (738, 6, NULL, 4, '2026-04-27 11:49:14.750927');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (739, 7, NULL, 2, '2026-04-27 11:49:14.763832');
INSERT INTO public.order_reservations (id, order_position_id, item_position_id, quantity, created_at) VALUES (3, 3, 1, 3, '2026-04-27 06:21:17.783148');


--
-- TOC entry 3950 (class 0 OID 16505)
-- Dependencies: 230
-- Data for Name: orders; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.orders (order_id, customer_id, branch_id, delivery_date, delivery_type, status, created_at, destination_address, payment_type, postamat_id, postamat_cell_id) VALUES (1, 1, 1, '2026-04-27 06:51:17.783148', 'Delivery', 'Created', '2026-04-27 06:21:17.783148', 'г. Москва, ул. Пушкина, 15', 'Prepaid', NULL, NULL);
INSERT INTO public.orders (order_id, customer_id, branch_id, delivery_date, delivery_type, status, created_at, destination_address, payment_type, postamat_id, postamat_cell_id) VALUES (3, 1, 1, '2026-04-27 07:51:17.783148', 'Postamat', 'Created', '2026-04-27 06:21:17.783148', 'Постамат №42, ТЦ Галерея', 'Prepaid', NULL, NULL);
INSERT INTO public.orders (order_id, customer_id, branch_id, delivery_date, delivery_type, status, created_at, destination_address, payment_type, postamat_id, postamat_cell_id) VALUES (4, 1, 3, '2026-04-27 07:06:17.783148', 'Express', 'Created', '2026-04-27 06:21:17.783148', 'г. Казань, ул. Баумана, 10', 'Prepaid', NULL, NULL);
INSERT INTO public.orders (order_id, customer_id, branch_id, delivery_date, delivery_type, status, created_at, destination_address, payment_type, postamat_id, postamat_cell_id) VALUES (5, 1, 1, '2026-04-27 06:36:17.783148', 'Pickup', 'Created', '2026-04-27 06:21:17.783148', NULL, 'Postpaid', NULL, NULL);
INSERT INTO public.orders (order_id, customer_id, branch_id, delivery_date, delivery_type, status, created_at, destination_address, payment_type, postamat_id, postamat_cell_id) VALUES (2, 1, 1, '2026-04-27 07:21:17.783148', 'Pickup', 'Assembly', '2026-04-27 06:21:17.783148', NULL, 'Postpaid', NULL, NULL);


--
-- TOC entry 3952 (class 0 OID 16524)
-- Dependencies: 232
-- Data for Name: positions; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) VALUES (1, 1, 'Active', 'A', 'Стеллаж', 'A-01', 'Полка 2', 'Ячейка 3', 100, 50, 40, '2026-04-12 07:28:35.283955');
INSERT INTO public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) VALUES (2, 1, 'Active', 'B', 'Паллет', 'B-05', NULL, NULL, 120, 80, 100, '2026-04-12 07:28:35.283955');
INSERT INTO public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) VALUES (3, 2, 'Active', 'C', 'Контейнер', 'C-12', 'Секция 1', NULL, 60, 40, 30, '2026-04-12 07:28:35.283955');
INSERT INTO public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) VALUES (70, 1, 'Reserved', 'PICKUP', 'Ячейка', 'P-03', NULL, NULL, 60, 40, 40, '2026-04-25 16:08:16.148282');
INSERT INTO public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) VALUES (68, 1, 'Reserved', 'PICKUP', 'Ячейка', 'P-01', NULL, NULL, 60, 40, 40, '2026-04-25 16:08:16.148282');
INSERT INTO public.positions (position_id, branch_id, status, zone_code, first_level_storage_type, fls_number, second_level_storage, third_level_storage, length, width, height, created_at) VALUES (69, 1, 'Active', 'PICKUP', 'Ячейка', 'P-02', NULL, NULL, 60, 40, 40, '2026-04-25 16:08:16.148282');


--
-- TOC entry 4003 (class 0 OID 51911)
-- Dependencies: 283
-- Data for Name: postamat_cells; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.postamat_cells (cell_id, postamat_id, cell_number, size_label, length, width, height, status) VALUES (1, 1, 'A1', 'XS', 150, 200, 100, 'Available');
INSERT INTO public.postamat_cells (cell_id, postamat_id, cell_number, size_label, length, width, height, status) VALUES (2, 1, 'A2', 'S', 200, 300, 150, 'Available');
INSERT INTO public.postamat_cells (cell_id, postamat_id, cell_number, size_label, length, width, height, status) VALUES (3, 1, 'B1', 'M', 400, 400, 300, 'Available');
INSERT INTO public.postamat_cells (cell_id, postamat_id, cell_number, size_label, length, width, height, status) VALUES (4, 1, 'B2', 'M', 400, 400, 300, 'Available');
INSERT INTO public.postamat_cells (cell_id, postamat_id, cell_number, size_label, length, width, height, status) VALUES (5, 1, 'C1', 'L', 600, 600, 600, 'Available');
INSERT INTO public.postamat_cells (cell_id, postamat_id, cell_number, size_label, length, width, height, status) VALUES (6, 1, 'D1', 'XL (Негабарит)', 1000, 800, 800, 'Available');


--
-- TOC entry 4001 (class 0 OID 51897)
-- Dependencies: 281
-- Data for Name: postamats; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.postamats (postamat_id, address, status, created_at) VALUES (1, 'ТЦ Галерея, ул. Ленина 5', 'Active', '2026-04-27 01:28:00.031576');


--
-- TOC entry 3944 (class 0 OID 16444)
-- Dependencies: 224
-- Data for Name: raw_events; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (1, 'scan', '{"barcode": "123456", "location": "A-01"}', '2025-07-16 10:30:00', 'ScannerService', '2026-04-12 07:28:35.291356');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (2, 'login', '{"device": "tablet", "user_id": 2}', '2025-07-16 09:00:00', 'AuthService', '2026-04-12 07:28:35.291356');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (3, 'scan', '{"barcode": "789012", "location": "B-05"}', '2025-07-16 11:15:00', 'ScannerService', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (4, 'logout', '{"device": "handheld", "user_id": 1}', '2025-07-16 17:35:00', 'AuthService', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (5, 'error', '{"code": "E404", "message": "Не найдено"}', '2025-07-17 10:20:00', 'InventoryService', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (6, 'update', '{"position": "A-01", "quantity": 15}', '2025-07-17 14:00:00', 'WMS', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (7, 'scan', '{"barcode": "345678", "location": "C-12"}', '2025-07-18 09:45:00', 'MobileApp', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (8, 'login', '{"device": "desktop", "user_id": 3}', '2025-07-18 08:30:00', 'AuthService', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (9, 'movement', '{"to": "B-05", "from": "A-01", "items": 5}', '2025-07-18 11:20:00', 'WMS', '2026-04-12 07:28:35.296657');
INSERT INTO public.raw_events (report_id, type, json_params, event_time, source_service, created_at) VALUES (10, 'alert', '{"type": "low_stock", "position": "C-12"}', '2025-07-18 15:40:00', 'Monitoring', '2026-04-12 07:28:35.296657');


--
-- TOC entry 3999 (class 0 OID 18969)
-- Dependencies: 279
-- Data for Name: worker_task_efficiency; Type: TABLE DATA; Schema: public; Owner: taskservice_user
--

INSERT INTO public.worker_task_efficiency (id, worker_id, branch_id, task_category, items_processed, total_duration_seconds, discrepancies_found, completed_at, wait_time_seconds, queue_size) VALUES (1, 1, 1, 'OrderAssembly', 3, 276, 0, '2026-04-24 09:49:09.59789', 0, 0);
INSERT INTO public.worker_task_efficiency (id, worker_id, branch_id, task_category, items_processed, total_duration_seconds, discrepancies_found, completed_at, wait_time_seconds, queue_size) VALUES (2, 1, 1, 'Inventory', 1, 1150, 0, '2026-04-25 01:38:52.4617', 0, 1);
INSERT INTO public.worker_task_efficiency (id, worker_id, branch_id, task_category, items_processed, total_duration_seconds, discrepancies_found, completed_at, wait_time_seconds, queue_size) VALUES (3, 1, 1, 'OrderAssembly', 3, 4766, 0, '2026-04-25 01:49:24.844224', 0, 0);
INSERT INTO public.worker_task_efficiency (id, worker_id, branch_id, task_category, items_processed, total_duration_seconds, discrepancies_found, completed_at, wait_time_seconds, queue_size) VALUES (4, 1, 1, 'OrderAssembly', 3, 766, 0, '2026-04-25 06:08:54.437417', 0, 0);
INSERT INTO public.worker_task_efficiency (id, worker_id, branch_id, task_category, items_processed, total_duration_seconds, discrepancies_found, completed_at, wait_time_seconds, queue_size) VALUES (5, 1, 1, 'OrderAssembly', 1, 12653, 0, '2026-04-25 10:32:55.91735', 0, 0);
INSERT INTO public.worker_task_efficiency (id, worker_id, branch_id, task_category, items_processed, total_duration_seconds, discrepancies_found, completed_at, wait_time_seconds, queue_size) VALUES (6, 1, 1, 'Inventory', 1, 1609, 0, '2026-04-25 11:00:00.182151', 0, 0);


--
-- TOC entry 4049 (class 0 OID 0)
-- Dependencies: 276
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.aggregatedcounter_id_seq', 1359, true);


--
-- TOC entry 4050 (class 0 OID 0)
-- Dependencies: 258
-- Name: counter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.counter_id_seq', 5344, true);


--
-- TOC entry 4051 (class 0 OID 0)
-- Dependencies: 260
-- Name: hash_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.hash_id_seq', 9, true);


--
-- TOC entry 4052 (class 0 OID 0)
-- Dependencies: 262
-- Name: job_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.job_id_seq', 1797, true);


--
-- TOC entry 4053 (class 0 OID 0)
-- Dependencies: 273
-- Name: jobparameter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.jobparameter_id_seq', 5330, true);


--
-- TOC entry 4054 (class 0 OID 0)
-- Dependencies: 266
-- Name: jobqueue_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.jobqueue_id_seq', 1811, true);


--
-- TOC entry 4055 (class 0 OID 0)
-- Dependencies: 268
-- Name: list_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.list_id_seq', 1, false);


--
-- TOC entry 4056 (class 0 OID 0)
-- Dependencies: 271
-- Name: set_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.set_id_seq', 1828, true);


--
-- TOC entry 4057 (class 0 OID 0)
-- Dependencies: 264
-- Name: state_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: taskservice_user
--

SELECT pg_catalog.setval('hangfire.state_id_seq', 5383, true);


--
-- TOC entry 4058 (class 0 OID 0)
-- Dependencies: 227
-- Name: active_assigned_tasks_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.active_assigned_tasks_id_seq', 1, false);


--
-- TOC entry 4059 (class 0 OID 0)
-- Dependencies: 225
-- Name: base_tasks_task_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.base_tasks_task_id_seq', 15, true);


--
-- TOC entry 4060 (class 0 OID 0)
-- Dependencies: 217
-- Name: branches_branch_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.branches_branch_id_seq', 3, true);


--
-- TOC entry 4061 (class 0 OID 0)
-- Dependencies: 221
-- Name: check_io_employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.check_io_employees_id_seq', 55, true);


--
-- TOC entry 4062 (class 0 OID 0)
-- Dependencies: 285
-- Name: customers_customer_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.customers_customer_id_seq', 1, false);


--
-- TOC entry 4063 (class 0 OID 0)
-- Dependencies: 215
-- Name: employees_employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.employees_employees_id_seq', 4, true);


--
-- TOC entry 4064 (class 0 OID 0)
-- Dependencies: 245
-- Name: inventory_assignment_lines_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_assignment_lines_id_seq', 1, false);


--
-- TOC entry 4065 (class 0 OID 0)
-- Dependencies: 243
-- Name: inventory_assignments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_assignments_id_seq', 1, false);


--
-- TOC entry 4066 (class 0 OID 0)
-- Dependencies: 247
-- Name: inventory_discrepancies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_discrepancies_id_seq', 1, false);


--
-- TOC entry 4067 (class 0 OID 0)
-- Dependencies: 249
-- Name: inventory_statistics_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.inventory_statistics_id_seq', 1, false);


--
-- TOC entry 4068 (class 0 OID 0)
-- Dependencies: 239
-- Name: item_movements_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.item_movements_id_seq', 1, false);


--
-- TOC entry 4069 (class 0 OID 0)
-- Dependencies: 235
-- Name: item_positions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.item_positions_id_seq', 1, true);


--
-- TOC entry 4070 (class 0 OID 0)
-- Dependencies: 241
-- Name: item_statuses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.item_statuses_id_seq', 1, false);


--
-- TOC entry 4071 (class 0 OID 0)
-- Dependencies: 219
-- Name: items_item_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.items_item_id_seq', 3, true);


--
-- TOC entry 4072 (class 0 OID 0)
-- Dependencies: 255
-- Name: mobile_app_users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.mobile_app_users_id_seq', 3, true);


--
-- TOC entry 4073 (class 0 OID 0)
-- Dependencies: 251
-- Name: order_assembly_assignments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.order_assembly_assignments_id_seq', 15, true);


--
-- TOC entry 4074 (class 0 OID 0)
-- Dependencies: 253
-- Name: order_assembly_lines_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.order_assembly_lines_id_seq', 30, true);


--
-- TOC entry 4075 (class 0 OID 0)
-- Dependencies: 233
-- Name: order_positions_unique_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.order_positions_unique_id_seq', 7, true);


--
-- TOC entry 4076 (class 0 OID 0)
-- Dependencies: 237
-- Name: order_reservations_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.order_reservations_id_seq', 739, true);


--
-- TOC entry 4077 (class 0 OID 0)
-- Dependencies: 229
-- Name: orders_order_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.orders_order_id_seq', 5, true);


--
-- TOC entry 4078 (class 0 OID 0)
-- Dependencies: 231
-- Name: positions_position_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.positions_position_id_seq', 70, true);


--
-- TOC entry 4079 (class 0 OID 0)
-- Dependencies: 282
-- Name: postamat_cells_cell_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.postamat_cells_cell_id_seq', 6, true);


--
-- TOC entry 4080 (class 0 OID 0)
-- Dependencies: 280
-- Name: postamats_postamat_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.postamats_postamat_id_seq', 1, true);


--
-- TOC entry 4081 (class 0 OID 0)
-- Dependencies: 223
-- Name: raw_events_report_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.raw_events_report_id_seq', 10, true);


--
-- TOC entry 4082 (class 0 OID 0)
-- Dependencies: 278
-- Name: worker_task_efficiency_id_seq; Type: SEQUENCE SET; Schema: public; Owner: taskservice_user
--

SELECT pg_catalog.setval('public.worker_task_efficiency_id_seq', 6, true);


--
-- TOC entry 3728 (class 2606 OID 17164)
-- Name: aggregatedcounter aggregatedcounter_key_key; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.aggregatedcounter
    ADD CONSTRAINT aggregatedcounter_key_key UNIQUE (key);


--
-- TOC entry 3730 (class 2606 OID 17162)
-- Name: aggregatedcounter aggregatedcounter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.aggregatedcounter
    ADD CONSTRAINT aggregatedcounter_pkey PRIMARY KEY (id);


--
-- TOC entry 3690 (class 2606 OID 16998)
-- Name: counter counter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.counter
    ADD CONSTRAINT counter_pkey PRIMARY KEY (id);


--
-- TOC entry 3694 (class 2606 OID 17133)
-- Name: hash hash_key_field_key; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.hash
    ADD CONSTRAINT hash_key_field_key UNIQUE (key, field);


--
-- TOC entry 3696 (class 2606 OID 17007)
-- Name: hash hash_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.hash
    ADD CONSTRAINT hash_pkey PRIMARY KEY (id);


--
-- TOC entry 3702 (class 2606 OID 17017)
-- Name: job job_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.job
    ADD CONSTRAINT job_pkey PRIMARY KEY (id);


--
-- TOC entry 3724 (class 2606 OID 17067)
-- Name: jobparameter jobparameter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.jobparameter
    ADD CONSTRAINT jobparameter_pkey PRIMARY KEY (id);


--
-- TOC entry 3710 (class 2606 OID 17090)
-- Name: jobqueue jobqueue_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.jobqueue
    ADD CONSTRAINT jobqueue_pkey PRIMARY KEY (id);


--
-- TOC entry 3713 (class 2606 OID 17110)
-- Name: list list_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.list
    ADD CONSTRAINT list_pkey PRIMARY KEY (id);


--
-- TOC entry 3726 (class 2606 OID 16989)
-- Name: lock lock_resource_key; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.lock
    ADD CONSTRAINT lock_resource_key UNIQUE (resource);

ALTER TABLE ONLY hangfire.lock REPLICA IDENTITY USING INDEX lock_resource_key;


--
-- TOC entry 3688 (class 2606 OID 16868)
-- Name: schema schema_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.schema
    ADD CONSTRAINT schema_pkey PRIMARY KEY (version);


--
-- TOC entry 3715 (class 2606 OID 17136)
-- Name: server server_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.server
    ADD CONSTRAINT server_pkey PRIMARY KEY (id);


--
-- TOC entry 3719 (class 2606 OID 17138)
-- Name: set set_key_value_key; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.set
    ADD CONSTRAINT set_key_value_key UNIQUE (key, value);


--
-- TOC entry 3721 (class 2606 OID 17119)
-- Name: set set_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.set
    ADD CONSTRAINT set_pkey PRIMARY KEY (id);


--
-- TOC entry 3705 (class 2606 OID 17044)
-- Name: state state_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.state
    ADD CONSTRAINT state_pkey PRIMARY KEY (id);


--
-- TOC entry 3601 (class 2606 OID 16491)
-- Name: active_assigned_tasks active_assigned_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks
    ADD CONSTRAINT active_assigned_tasks_pkey PRIMARY KEY (id);


--
-- TOC entry 3594 (class 2606 OID 16470)
-- Name: base_tasks base_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.base_tasks
    ADD CONSTRAINT base_tasks_pkey PRIMARY KEY (task_id);


--
-- TOC entry 3575 (class 2606 OID 16404)
-- Name: branches branches_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.branches
    ADD CONSTRAINT branches_pkey PRIMARY KEY (branch_id);


--
-- TOC entry 3582 (class 2606 OID 16428)
-- Name: check_io_employees check_io_employees_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees
    ADD CONSTRAINT check_io_employees_pkey PRIMARY KEY (id);


--
-- TOC entry 3743 (class 2606 OID 52098)
-- Name: courier_capabilities courier_capabilities_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.courier_capabilities
    ADD CONSTRAINT courier_capabilities_pkey PRIMARY KEY (employee_id);


--
-- TOC entry 3746 (class 2606 OID 52128)
-- Name: customers customers_email_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_email_key UNIQUE (email);


--
-- TOC entry 3748 (class 2606 OID 52126)
-- Name: customers customers_phone_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_phone_key UNIQUE (phone);


--
-- TOC entry 3750 (class 2606 OID 52124)
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (customer_id);


--
-- TOC entry 3571 (class 2606 OID 16392)
-- Name: employees employees_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_pkey PRIMARY KEY (employees_id);


--
-- TOC entry 3650 (class 2606 OID 16693)
-- Name: inventory_assignment_lines inventory_assignment_lines_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_pkey PRIMARY KEY (id);


--
-- TOC entry 3644 (class 2606 OID 16666)
-- Name: inventory_assignments inventory_assignments_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_pkey PRIMARY KEY (id);


--
-- TOC entry 3657 (class 2606 OID 16727)
-- Name: inventory_discrepancies inventory_discrepancies_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies
    ADD CONSTRAINT inventory_discrepancies_pkey PRIMARY KEY (id);


--
-- TOC entry 3662 (class 2606 OID 16759)
-- Name: inventory_statistics inventory_statistics_inventory_assignment_id_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics
    ADD CONSTRAINT inventory_statistics_inventory_assignment_id_key UNIQUE (inventory_assignment_id);


--
-- TOC entry 3664 (class 2606 OID 16757)
-- Name: inventory_statistics inventory_statistics_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics
    ADD CONSTRAINT inventory_statistics_pkey PRIMARY KEY (id);


--
-- TOC entry 3633 (class 2606 OID 16615)
-- Name: item_movements item_movements_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_pkey PRIMARY KEY (id);


--
-- TOC entry 3623 (class 2606 OID 16572)
-- Name: item_positions item_positions_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions
    ADD CONSTRAINT item_positions_pkey PRIMARY KEY (id);


--
-- TOC entry 3638 (class 2606 OID 16649)
-- Name: item_statuses item_statuses_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_statuses
    ADD CONSTRAINT item_statuses_pkey PRIMARY KEY (id);


--
-- TOC entry 3580 (class 2606 OID 16418)
-- Name: items items_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_pkey PRIMARY KEY (item_id);


--
-- TOC entry 3684 (class 2606 OID 16848)
-- Name: mobile_app_users mobile_app_users_employee_id_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users
    ADD CONSTRAINT mobile_app_users_employee_id_key UNIQUE (employee_id);


--
-- TOC entry 3686 (class 2606 OID 16846)
-- Name: mobile_app_users mobile_app_users_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users
    ADD CONSTRAINT mobile_app_users_pkey PRIMARY KEY (id);


--
-- TOC entry 3671 (class 2606 OID 16776)
-- Name: order_assembly_assignments order_assembly_assignments_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_assignments
    ADD CONSTRAINT order_assembly_assignments_pkey PRIMARY KEY (id);


--
-- TOC entry 3678 (class 2606 OID 16810)
-- Name: order_assembly_lines order_assembly_lines_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_lines
    ADD CONSTRAINT order_assembly_lines_pkey PRIMARY KEY (id);


--
-- TOC entry 3618 (class 2606 OID 16551)
-- Name: order_positions order_positions_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions
    ADD CONSTRAINT order_positions_pkey PRIMARY KEY (unique_id);


--
-- TOC entry 3627 (class 2606 OID 16594)
-- Name: order_reservations order_reservations_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_reservations
    ADD CONSTRAINT order_reservations_pkey PRIMARY KEY (id);


--
-- TOC entry 3609 (class 2606 OID 16513)
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (order_id);


--
-- TOC entry 3614 (class 2606 OID 16534)
-- Name: positions positions_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT positions_pkey PRIMARY KEY (position_id);


--
-- TOC entry 3741 (class 2606 OID 51921)
-- Name: postamat_cells postamat_cells_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.postamat_cells
    ADD CONSTRAINT postamat_cells_pkey PRIMARY KEY (cell_id);


--
-- TOC entry 3736 (class 2606 OID 51909)
-- Name: postamats postamats_address_key; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.postamats
    ADD CONSTRAINT postamats_address_key UNIQUE (address);


--
-- TOC entry 3738 (class 2606 OID 51907)
-- Name: postamats postamats_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.postamats
    ADD CONSTRAINT postamats_pkey PRIMARY KEY (postamat_id);


--
-- TOC entry 3592 (class 2606 OID 16452)
-- Name: raw_events raw_events_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.raw_events
    ADD CONSTRAINT raw_events_pkey PRIMARY KEY (report_id);


--
-- TOC entry 3734 (class 2606 OID 18978)
-- Name: worker_task_efficiency worker_task_efficiency_pkey; Type: CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.worker_task_efficiency
    ADD CONSTRAINT worker_task_efficiency_pkey PRIMARY KEY (id);


--
-- TOC entry 3691 (class 1259 OID 17165)
-- Name: ix_hangfire_counter_expireat; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_counter_expireat ON hangfire.counter USING btree (expireat);


--
-- TOC entry 3692 (class 1259 OID 17127)
-- Name: ix_hangfire_counter_key; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_counter_key ON hangfire.counter USING btree (key);


--
-- TOC entry 3697 (class 1259 OID 17166)
-- Name: ix_hangfire_hash_expireat; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_hash_expireat ON hangfire.hash USING btree (expireat);


--
-- TOC entry 3698 (class 1259 OID 17167)
-- Name: ix_hangfire_job_expireat; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_job_expireat ON hangfire.job USING btree (expireat);


--
-- TOC entry 3699 (class 1259 OID 17134)
-- Name: ix_hangfire_job_statename; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_job_statename ON hangfire.job USING btree (statename);


--
-- TOC entry 3700 (class 1259 OID 17202)
-- Name: ix_hangfire_job_statename_is_not_null; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_job_statename_is_not_null ON hangfire.job USING btree (statename) INCLUDE (id) WHERE (statename IS NOT NULL);


--
-- TOC entry 3722 (class 1259 OID 17139)
-- Name: ix_hangfire_jobparameter_jobidandname; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_jobparameter_jobidandname ON hangfire.jobparameter USING btree (jobid, name);


--
-- TOC entry 3706 (class 1259 OID 17201)
-- Name: ix_hangfire_jobqueue_fetchedat_queue_jobid; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_jobqueue_fetchedat_queue_jobid ON hangfire.jobqueue USING btree (fetchedat NULLS FIRST, queue, jobid);


--
-- TOC entry 3707 (class 1259 OID 17099)
-- Name: ix_hangfire_jobqueue_jobidandqueue; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_jobqueue_jobidandqueue ON hangfire.jobqueue USING btree (jobid, queue);


--
-- TOC entry 3708 (class 1259 OID 17168)
-- Name: ix_hangfire_jobqueue_queueandfetchedat; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_jobqueue_queueandfetchedat ON hangfire.jobqueue USING btree (queue, fetchedat);


--
-- TOC entry 3711 (class 1259 OID 17170)
-- Name: ix_hangfire_list_expireat; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_list_expireat ON hangfire.list USING btree (expireat);


--
-- TOC entry 3716 (class 1259 OID 17171)
-- Name: ix_hangfire_set_expireat; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_set_expireat ON hangfire.set USING btree (expireat);


--
-- TOC entry 3717 (class 1259 OID 17153)
-- Name: ix_hangfire_set_key_score; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_set_key_score ON hangfire.set USING btree (key, score);


--
-- TOC entry 3703 (class 1259 OID 17052)
-- Name: ix_hangfire_state_jobid; Type: INDEX; Schema: hangfire; Owner: taskservice_user
--

CREATE INDEX ix_hangfire_state_jobid ON hangfire.state USING btree (jobid);


--
-- TOC entry 3602 (class 1259 OID 16503)
-- Name: idx_assigned_tasks_combo; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_assigned_tasks_combo ON public.active_assigned_tasks USING btree (task_id, user_id);


--
-- TOC entry 3603 (class 1259 OID 16502)
-- Name: idx_assigned_tasks_user; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_assigned_tasks_user ON public.active_assigned_tasks USING btree (user_id);


--
-- TOC entry 3595 (class 1259 OID 16476)
-- Name: idx_base_tasks_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_branch ON public.base_tasks USING btree (branch_id);


--
-- TOC entry 3596 (class 1259 OID 16481)
-- Name: idx_base_tasks_completed; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_completed ON public.base_tasks USING btree (completed_at);


--
-- TOC entry 3597 (class 1259 OID 16480)
-- Name: idx_base_tasks_created; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_created ON public.base_tasks USING btree (created_at);


--
-- TOC entry 3598 (class 1259 OID 16477)
-- Name: idx_base_tasks_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_status ON public.base_tasks USING btree (status);


--
-- TOC entry 3599 (class 1259 OID 16478)
-- Name: idx_base_tasks_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_base_tasks_type ON public.base_tasks USING btree (type);


--
-- TOC entry 3576 (class 1259 OID 16405)
-- Name: idx_branches_name; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE UNIQUE INDEX idx_branches_name ON public.branches USING btree (branch_name);


--
-- TOC entry 3577 (class 1259 OID 16406)
-- Name: idx_branches_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_branches_type ON public.branches USING btree (branch_type);


--
-- TOC entry 3583 (class 1259 OID 16440)
-- Name: idx_check_io_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_branch ON public.check_io_employees USING btree (branch_id);


--
-- TOC entry 3584 (class 1259 OID 16439)
-- Name: idx_check_io_employee; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_employee ON public.check_io_employees USING btree (employee_id);


--
-- TOC entry 3585 (class 1259 OID 16441)
-- Name: idx_check_io_timestamp; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_timestamp ON public.check_io_employees USING btree (check_timestamp);


--
-- TOC entry 3586 (class 1259 OID 16442)
-- Name: idx_check_io_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_check_io_type ON public.check_io_employees USING btree (check_type);


--
-- TOC entry 3744 (class 1259 OID 52104)
-- Name: idx_courier_capabilities_vehicle; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_courier_capabilities_vehicle ON public.courier_capabilities USING btree (vehicle_type_id);


--
-- TOC entry 3651 (class 1259 OID 16742)
-- Name: idx_discrepancies_identified; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_identified ON public.inventory_discrepancies USING btree (identified_at);


--
-- TOC entry 3652 (class 1259 OID 16739)
-- Name: idx_discrepancies_itemposition; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_itemposition ON public.inventory_discrepancies USING btree (item_position_id);


--
-- TOC entry 3653 (class 1259 OID 16738)
-- Name: idx_discrepancies_line; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_line ON public.inventory_discrepancies USING btree (inventory_assignment_line_id);


--
-- TOC entry 3654 (class 1259 OID 16741)
-- Name: idx_discrepancies_resolution; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_resolution ON public.inventory_discrepancies USING btree (resolution_status);


--
-- TOC entry 3655 (class 1259 OID 16740)
-- Name: idx_discrepancies_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_discrepancies_type ON public.inventory_discrepancies USING btree (type);


--
-- TOC entry 3572 (class 1259 OID 16393)
-- Name: idx_employees_name; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_employees_name ON public.employees USING btree (surname, name);


--
-- TOC entry 3573 (class 1259 OID 52088)
-- Name: idx_employees_role_id; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_employees_role_id ON public.employees USING btree (role_id);


--
-- TOC entry 3639 (class 1259 OID 16684)
-- Name: idx_inventory_assignments_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_branch ON public.inventory_assignments USING btree (branch_id);


--
-- TOC entry 3640 (class 1259 OID 16685)
-- Name: idx_inventory_assignments_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_status ON public.inventory_assignments USING btree (status);


--
-- TOC entry 3641 (class 1259 OID 16682)
-- Name: idx_inventory_assignments_task; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_task ON public.inventory_assignments USING btree (task_id);


--
-- TOC entry 3642 (class 1259 OID 16683)
-- Name: idx_inventory_assignments_user; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_assignments_user ON public.inventory_assignments USING btree (assigned_to_user_id);


--
-- TOC entry 3645 (class 1259 OID 16709)
-- Name: idx_inventory_lines_assignment; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_assignment ON public.inventory_assignment_lines USING btree (inventory_assignment_id);


--
-- TOC entry 3646 (class 1259 OID 16710)
-- Name: idx_inventory_lines_itemposition; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_itemposition ON public.inventory_assignment_lines USING btree (item_position_id);


--
-- TOC entry 3647 (class 1259 OID 16711)
-- Name: idx_inventory_lines_position; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_position ON public.inventory_assignment_lines USING btree (position_id);


--
-- TOC entry 3648 (class 1259 OID 16712)
-- Name: idx_inventory_lines_zone; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_inventory_lines_zone ON public.inventory_assignment_lines USING btree (zone_code);


--
-- TOC entry 3628 (class 1259 OID 16639)
-- Name: idx_item_movements_destination_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_destination_branch ON public.item_movements USING btree (destination_branch_id);


--
-- TOC entry 3629 (class 1259 OID 16637)
-- Name: idx_item_movements_destination_pos; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_destination_pos ON public.item_movements USING btree (destination_position_id);


--
-- TOC entry 3630 (class 1259 OID 16638)
-- Name: idx_item_movements_source_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_source_branch ON public.item_movements USING btree (source_branch_id);


--
-- TOC entry 3631 (class 1259 OID 16636)
-- Name: idx_item_movements_source_itempos; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_movements_source_itempos ON public.item_movements USING btree (source_item_position_id);


--
-- TOC entry 3619 (class 1259 OID 16585)
-- Name: idx_item_positions_id; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE UNIQUE INDEX idx_item_positions_id ON public.item_positions USING btree (id);


--
-- TOC entry 3620 (class 1259 OID 16584)
-- Name: idx_item_positions_item; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_positions_item ON public.item_positions USING btree (item_id);


--
-- TOC entry 3621 (class 1259 OID 16583)
-- Name: idx_item_positions_position; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_positions_position ON public.item_positions USING btree (position_id);


--
-- TOC entry 3634 (class 1259 OID 16657)
-- Name: idx_item_statuses_date; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_statuses_date ON public.item_statuses USING btree (status_date);


--
-- TOC entry 3635 (class 1259 OID 16656)
-- Name: idx_item_statuses_position; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_statuses_position ON public.item_statuses USING btree (item_position_id);


--
-- TOC entry 3636 (class 1259 OID 16655)
-- Name: idx_item_statuses_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_item_statuses_status ON public.item_statuses USING btree (status);


--
-- TOC entry 3578 (class 1259 OID 16419)
-- Name: idx_items_dimensions; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_items_dimensions ON public.items USING btree (length, width, height);


--
-- TOC entry 3679 (class 1259 OID 16856)
-- Name: idx_mobile_app_users_active; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_active ON public.mobile_app_users USING btree (is_active);


--
-- TOC entry 3680 (class 1259 OID 16854)
-- Name: idx_mobile_app_users_employee; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_employee ON public.mobile_app_users USING btree (employee_id);


--
-- TOC entry 3681 (class 1259 OID 52105)
-- Name: idx_mobile_app_users_role; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_role ON public.mobile_app_users USING btree (role);


--
-- TOC entry 3682 (class 1259 OID 52116)
-- Name: idx_mobile_app_users_role_id; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_mobile_app_users_role_id ON public.mobile_app_users USING btree (role);


--
-- TOC entry 3665 (class 1259 OID 16800)
-- Name: idx_order_assembly_assignments_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_assignments_branch ON public.order_assembly_assignments USING btree (branch_id);


--
-- TOC entry 3666 (class 1259 OID 16798)
-- Name: idx_order_assembly_assignments_order; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_assignments_order ON public.order_assembly_assignments USING btree (order_id);


--
-- TOC entry 3667 (class 1259 OID 16801)
-- Name: idx_order_assembly_assignments_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_assignments_status ON public.order_assembly_assignments USING btree (status);


--
-- TOC entry 3668 (class 1259 OID 16797)
-- Name: idx_order_assembly_assignments_task; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_assignments_task ON public.order_assembly_assignments USING btree (task_id);


--
-- TOC entry 3669 (class 1259 OID 16799)
-- Name: idx_order_assembly_assignments_user; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_assignments_user ON public.order_assembly_assignments USING btree (assigned_to_user_id);


--
-- TOC entry 3672 (class 1259 OID 16831)
-- Name: idx_order_assembly_lines_assignment; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_lines_assignment ON public.order_assembly_lines USING btree (order_assembly_assignment_id);


--
-- TOC entry 3673 (class 1259 OID 16832)
-- Name: idx_order_assembly_lines_itemposition; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_lines_itemposition ON public.order_assembly_lines USING btree (item_position_id);


--
-- TOC entry 3674 (class 1259 OID 16833)
-- Name: idx_order_assembly_lines_source; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_lines_source ON public.order_assembly_lines USING btree (source_position_id);


--
-- TOC entry 3675 (class 1259 OID 16835)
-- Name: idx_order_assembly_lines_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_lines_status ON public.order_assembly_lines USING btree (status);


--
-- TOC entry 3676 (class 1259 OID 16834)
-- Name: idx_order_assembly_lines_target; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_assembly_lines_target ON public.order_assembly_lines USING btree (target_position_id);


--
-- TOC entry 3615 (class 1259 OID 16563)
-- Name: idx_order_positions_item; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_positions_item ON public.order_positions USING btree (item_id);


--
-- TOC entry 3616 (class 1259 OID 16562)
-- Name: idx_order_positions_order; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_positions_order ON public.order_positions USING btree (order_id);


--
-- TOC entry 3624 (class 1259 OID 16606)
-- Name: idx_order_reservations_item_pos; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_reservations_item_pos ON public.order_reservations USING btree (item_position_id);


--
-- TOC entry 3625 (class 1259 OID 16605)
-- Name: idx_order_reservations_order_pos; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_order_reservations_order_pos ON public.order_reservations USING btree (order_position_id);


--
-- TOC entry 3604 (class 1259 OID 16520)
-- Name: idx_orders_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_branch ON public.orders USING btree (branch_id);


--
-- TOC entry 3605 (class 1259 OID 16519)
-- Name: idx_orders_customer; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_customer ON public.orders USING btree (customer_id);


--
-- TOC entry 3606 (class 1259 OID 16522)
-- Name: idx_orders_delivery_date; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_delivery_date ON public.orders USING btree (delivery_date);


--
-- TOC entry 3607 (class 1259 OID 16521)
-- Name: idx_orders_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_orders_status ON public.orders USING btree (status);


--
-- TOC entry 3610 (class 1259 OID 16540)
-- Name: idx_positions_branch; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_positions_branch ON public.positions USING btree (branch_id);


--
-- TOC entry 3611 (class 1259 OID 16541)
-- Name: idx_positions_status; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_positions_status ON public.positions USING btree (status);


--
-- TOC entry 3612 (class 1259 OID 16542)
-- Name: idx_positions_zone; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_positions_zone ON public.positions USING btree (zone_code);


--
-- TOC entry 3739 (class 1259 OID 51927)
-- Name: idx_postamat_cells_search; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_postamat_cells_search ON public.postamat_cells USING btree (postamat_id, status);


--
-- TOC entry 3587 (class 1259 OID 16455)
-- Name: idx_raw_events_event_time; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_event_time ON public.raw_events USING btree (event_time);


--
-- TOC entry 3588 (class 1259 OID 16456)
-- Name: idx_raw_events_params; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_params ON public.raw_events USING gin (json_params);


--
-- TOC entry 3589 (class 1259 OID 16454)
-- Name: idx_raw_events_service; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_service ON public.raw_events USING btree (source_service);


--
-- TOC entry 3590 (class 1259 OID 16453)
-- Name: idx_raw_events_type; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_raw_events_type ON public.raw_events USING btree (type);


--
-- TOC entry 3658 (class 1259 OID 16765)
-- Name: idx_statistics_assignment; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_statistics_assignment ON public.inventory_statistics USING btree (inventory_assignment_id);


--
-- TOC entry 3659 (class 1259 OID 16767)
-- Name: idx_statistics_completed; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_statistics_completed ON public.inventory_statistics USING btree (completed_at);


--
-- TOC entry 3660 (class 1259 OID 16766)
-- Name: idx_statistics_started; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_statistics_started ON public.inventory_statistics USING btree (started_at);


--
-- TOC entry 3731 (class 1259 OID 18980)
-- Name: idx_worker_efficiency_branch_date; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_worker_efficiency_branch_date ON public.worker_task_efficiency USING btree (branch_id, completed_at);


--
-- TOC entry 3732 (class 1259 OID 18979)
-- Name: idx_worker_efficiency_worker_date; Type: INDEX; Schema: public; Owner: taskservice_user
--

CREATE INDEX idx_worker_efficiency_worker_date ON public.worker_task_efficiency USING btree (worker_id, completed_at);


--
-- TOC entry 3790 (class 2606 OID 17076)
-- Name: jobparameter jobparameter_jobid_fkey; Type: FK CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.jobparameter
    ADD CONSTRAINT jobparameter_jobid_fkey FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3789 (class 2606 OID 17053)
-- Name: state state_jobid_fkey; Type: FK CONSTRAINT; Schema: hangfire; Owner: taskservice_user
--

ALTER TABLE ONLY hangfire.state
    ADD CONSTRAINT state_jobid_fkey FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3754 (class 2606 OID 16492)
-- Name: active_assigned_tasks active_assigned_tasks_task_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks
    ADD CONSTRAINT active_assigned_tasks_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.base_tasks(task_id);


--
-- TOC entry 3755 (class 2606 OID 16497)
-- Name: active_assigned_tasks active_assigned_tasks_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.active_assigned_tasks
    ADD CONSTRAINT active_assigned_tasks_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.employees(employees_id);


--
-- TOC entry 3753 (class 2606 OID 16471)
-- Name: base_tasks base_tasks_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.base_tasks
    ADD CONSTRAINT base_tasks_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3751 (class 2606 OID 16434)
-- Name: check_io_employees check_io_employees_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees
    ADD CONSTRAINT check_io_employees_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3752 (class 2606 OID 16429)
-- Name: check_io_employees check_io_employees_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.check_io_employees
    ADD CONSTRAINT check_io_employees_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employees_id);


--
-- TOC entry 3792 (class 2606 OID 52099)
-- Name: courier_capabilities courier_capabilities_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.courier_capabilities
    ADD CONSTRAINT courier_capabilities_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employees_id) ON DELETE CASCADE;


--
-- TOC entry 3774 (class 2606 OID 16694)
-- Name: inventory_assignment_lines inventory_assignment_lines_inventory_assignment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_inventory_assignment_id_fkey FOREIGN KEY (inventory_assignment_id) REFERENCES public.inventory_assignments(id);


--
-- TOC entry 3775 (class 2606 OID 16699)
-- Name: inventory_assignment_lines inventory_assignment_lines_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- TOC entry 3776 (class 2606 OID 16704)
-- Name: inventory_assignment_lines inventory_assignment_lines_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignment_lines
    ADD CONSTRAINT inventory_assignment_lines_position_id_fkey FOREIGN KEY (position_id) REFERENCES public.positions(position_id);


--
-- TOC entry 3771 (class 2606 OID 16672)
-- Name: inventory_assignments inventory_assignments_assigned_to_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_assigned_to_user_id_fkey FOREIGN KEY (assigned_to_user_id) REFERENCES public.employees(employees_id);


--
-- TOC entry 3772 (class 2606 OID 16677)
-- Name: inventory_assignments inventory_assignments_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3773 (class 2606 OID 16667)
-- Name: inventory_assignments inventory_assignments_task_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_assignments
    ADD CONSTRAINT inventory_assignments_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.base_tasks(task_id);


--
-- TOC entry 3777 (class 2606 OID 16728)
-- Name: inventory_discrepancies inventory_discrepancies_inventory_assignment_line_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies
    ADD CONSTRAINT inventory_discrepancies_inventory_assignment_line_id_fkey FOREIGN KEY (inventory_assignment_line_id) REFERENCES public.inventory_assignment_lines(id);


--
-- TOC entry 3778 (class 2606 OID 16733)
-- Name: inventory_discrepancies inventory_discrepancies_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_discrepancies
    ADD CONSTRAINT inventory_discrepancies_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- TOC entry 3779 (class 2606 OID 16760)
-- Name: inventory_statistics inventory_statistics_inventory_assignment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.inventory_statistics
    ADD CONSTRAINT inventory_statistics_inventory_assignment_id_fkey FOREIGN KEY (inventory_assignment_id) REFERENCES public.inventory_assignments(id);


--
-- TOC entry 3766 (class 2606 OID 16631)
-- Name: item_movements item_movements_destination_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_destination_branch_id_fkey FOREIGN KEY (destination_branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3767 (class 2606 OID 16621)
-- Name: item_movements item_movements_destination_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_destination_position_id_fkey FOREIGN KEY (destination_position_id) REFERENCES public.positions(position_id);


--
-- TOC entry 3768 (class 2606 OID 16626)
-- Name: item_movements item_movements_source_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_source_branch_id_fkey FOREIGN KEY (source_branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3769 (class 2606 OID 16616)
-- Name: item_movements item_movements_source_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_movements
    ADD CONSTRAINT item_movements_source_item_position_id_fkey FOREIGN KEY (source_item_position_id) REFERENCES public.item_positions(id);


--
-- TOC entry 3762 (class 2606 OID 16573)
-- Name: item_positions item_positions_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions
    ADD CONSTRAINT item_positions_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(item_id);


--
-- TOC entry 3763 (class 2606 OID 16578)
-- Name: item_positions item_positions_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_positions
    ADD CONSTRAINT item_positions_position_id_fkey FOREIGN KEY (position_id) REFERENCES public.positions(position_id);


--
-- TOC entry 3770 (class 2606 OID 16650)
-- Name: item_statuses item_statuses_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.item_statuses
    ADD CONSTRAINT item_statuses_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- TOC entry 3788 (class 2606 OID 16849)
-- Name: mobile_app_users mobile_app_users_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.mobile_app_users
    ADD CONSTRAINT mobile_app_users_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employees_id);


--
-- TOC entry 3780 (class 2606 OID 16787)
-- Name: order_assembly_assignments order_assembly_assignments_assigned_to_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_assignments
    ADD CONSTRAINT order_assembly_assignments_assigned_to_user_id_fkey FOREIGN KEY (assigned_to_user_id) REFERENCES public.employees(employees_id);


--
-- TOC entry 3781 (class 2606 OID 16792)
-- Name: order_assembly_assignments order_assembly_assignments_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_assignments
    ADD CONSTRAINT order_assembly_assignments_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3782 (class 2606 OID 16782)
-- Name: order_assembly_assignments order_assembly_assignments_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_assignments
    ADD CONSTRAINT order_assembly_assignments_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(order_id);


--
-- TOC entry 3783 (class 2606 OID 16777)
-- Name: order_assembly_assignments order_assembly_assignments_task_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_assignments
    ADD CONSTRAINT order_assembly_assignments_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.base_tasks(task_id);


--
-- TOC entry 3784 (class 2606 OID 16816)
-- Name: order_assembly_lines order_assembly_lines_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_lines
    ADD CONSTRAINT order_assembly_lines_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- TOC entry 3785 (class 2606 OID 16811)
-- Name: order_assembly_lines order_assembly_lines_order_assembly_assignment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_lines
    ADD CONSTRAINT order_assembly_lines_order_assembly_assignment_id_fkey FOREIGN KEY (order_assembly_assignment_id) REFERENCES public.order_assembly_assignments(id);


--
-- TOC entry 3786 (class 2606 OID 16821)
-- Name: order_assembly_lines order_assembly_lines_source_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_lines
    ADD CONSTRAINT order_assembly_lines_source_position_id_fkey FOREIGN KEY (source_position_id) REFERENCES public.positions(position_id);


--
-- TOC entry 3787 (class 2606 OID 16826)
-- Name: order_assembly_lines order_assembly_lines_target_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_assembly_lines
    ADD CONSTRAINT order_assembly_lines_target_position_id_fkey FOREIGN KEY (target_position_id) REFERENCES public.positions(position_id);


--
-- TOC entry 3760 (class 2606 OID 16557)
-- Name: order_positions order_positions_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions
    ADD CONSTRAINT order_positions_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(item_id);


--
-- TOC entry 3761 (class 2606 OID 16552)
-- Name: order_positions order_positions_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_positions
    ADD CONSTRAINT order_positions_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(order_id);


--
-- TOC entry 3764 (class 2606 OID 16600)
-- Name: order_reservations order_reservations_item_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_reservations
    ADD CONSTRAINT order_reservations_item_position_id_fkey FOREIGN KEY (item_position_id) REFERENCES public.item_positions(id);


--
-- TOC entry 3765 (class 2606 OID 16595)
-- Name: order_reservations order_reservations_order_position_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.order_reservations
    ADD CONSTRAINT order_reservations_order_position_id_fkey FOREIGN KEY (order_position_id) REFERENCES public.order_positions(unique_id);


--
-- TOC entry 3756 (class 2606 OID 16514)
-- Name: orders orders_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3757 (class 2606 OID 51933)
-- Name: orders orders_postamat_cell_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_postamat_cell_id_fkey FOREIGN KEY (postamat_cell_id) REFERENCES public.postamat_cells(cell_id);


--
-- TOC entry 3758 (class 2606 OID 51928)
-- Name: orders orders_postamat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_postamat_id_fkey FOREIGN KEY (postamat_id) REFERENCES public.postamats(postamat_id);


--
-- TOC entry 3759 (class 2606 OID 16535)
-- Name: positions positions_branch_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT positions_branch_id_fkey FOREIGN KEY (branch_id) REFERENCES public.branches(branch_id);


--
-- TOC entry 3791 (class 2606 OID 51922)
-- Name: postamat_cells postamat_cells_postamat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: taskservice_user
--

ALTER TABLE ONLY public.postamat_cells
    ADD CONSTRAINT postamat_cells_postamat_id_fkey FOREIGN KEY (postamat_id) REFERENCES public.postamats(postamat_id) ON DELETE CASCADE;


-- Completed on 2026-04-27 12:28:32 UTC

--
-- PostgreSQL database dump complete
--

\unrestrict dotCF4mkaHZLbltkFqmiUuAyDTJMSFZEI2zcfZkoy599w8jKapWbtrKiFsk85bO

