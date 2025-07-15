

-- Создание таблицы сотрудников
CREATE TABLE IF NOT EXISTS employees (
    employees_id SERIAL PRIMARY KEY,
    surname VARCHAR(100) NOT NULL,
    name VARCHAR(100) NOT NULL,
    middle_name VARCHAR(100),
    role VARCHAR(150) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для сотрудников
CREATE INDEX idx_employees_name ON employees (surname, name);
CREATE INDEX idx_employees_role ON employees (role);

-- Создание таблицы филиалов
CREATE TABLE IF NOT EXISTS branches (
    branch_id SERIAL PRIMARY KEY,
    branch_name VARCHAR(200) NOT NULL,
    branch_type VARCHAR(100) NOT NULL,
    address VARCHAR(500) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Уникальный индекс для названий филиалов
CREATE UNIQUE INDEX idx_branches_name ON branches (branch_name);
CREATE INDEX idx_branches_type ON branches (branch_type);

-- Создание таблицы товаров
CREATE TABLE IF NOT EXISTS items (
    item_id SERIAL PRIMARY KEY,
    weight DOUBLE PRECISION NOT NULL CHECK (weight > 0),
    length DOUBLE PRECISION NOT NULL CHECK (length > 0),
    width DOUBLE PRECISION NOT NULL CHECK (width > 0),
    height DOUBLE PRECISION NOT NULL CHECK (height > 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индекс по размерам товара
CREATE INDEX idx_items_dimensions ON items (length, width, height);

-- Создание таблицы отметок сотрудников
CREATE TABLE IF NOT EXISTS check_io_employees (
    id SERIAL PRIMARY KEY,
    employee_id INT NOT NULL REFERENCES employees(employees_id),
    branch_id INT NOT NULL REFERENCES branches(branch_id),
    check_type VARCHAR(3) NOT NULL CHECK (check_type IN ('in', 'out')),
    check_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для быстрого поиска отметок
CREATE INDEX idx_check_io_employee ON check_io_employees (employee_id);
CREATE INDEX idx_check_io_branch ON check_io_employees (branch_id);
CREATE INDEX idx_check_io_timestamp ON check_io_employees (check_timestamp);
CREATE INDEX idx_check_io_type ON check_io_employees (check_type);

-- Создание таблицы сырых событий
CREATE TABLE IF NOT EXISTS raw_events (
    report_id SERIAL PRIMARY KEY,
    type VARCHAR(50) NOT NULL,
    json_params JSONB NOT NULL,
    event_time TIMESTAMP NOT NULL,
    source_service VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для JSON и типов событий
CREATE INDEX idx_raw_events_type ON raw_events (type);
CREATE INDEX idx_raw_events_service ON raw_events (source_service);
CREATE INDEX idx_raw_events_event_time ON raw_events (event_time);
CREATE INDEX idx_raw_events_params ON raw_events USING GIN (json_params);

-- Создание таблицы активных задач
CREATE TABLE IF NOT EXISTS active_tasks (
    task_id SERIAL PRIMARY KEY,
    branch_id INT NOT NULL REFERENCES branches(branch_id),
    type VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP,
    status VARCHAR(20) NOT NULL CHECK (status IN ('New', 'InProgress', 'Completed', 'Cancelled')),
    json_params JSONB
);

-- Индексы для задач
CREATE INDEX idx_active_tasks_branch ON active_tasks (branch_id);
CREATE INDEX idx_active_tasks_status ON active_tasks (status);
CREATE INDEX idx_active_tasks_type ON active_tasks (type);
CREATE INDEX idx_active_tasks_created ON active_tasks (created_at);

-- Создание таблицы назначенных задач
CREATE TABLE IF NOT EXISTS active_assigned_tasks (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL REFERENCES active_tasks(task_id),
    user_id INT NOT NULL REFERENCES employees(employees_id),
    assigned_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Составной индекс для назначений
CREATE INDEX idx_assigned_tasks_user ON active_assigned_tasks (user_id);
CREATE INDEX idx_assigned_tasks_combo ON active_assigned_tasks (task_id, user_id);

-- Создание таблицы заказов
CREATE TABLE IF NOT EXISTS orders (
    order_id SERIAL PRIMARY KEY,
    customer_id INT NOT NULL,
    branch_id INT NOT NULL REFERENCES branches(branch_id),
    delivery_date TIMESTAMP,
    type VARCHAR(20) NOT NULL CHECK (type IN ('Online', 'Offline', 'Wholesale')),
    status VARCHAR(20) NOT NULL CHECK (status IN ('New', 'Processing', 'Shipped', 'Delivered', 'Cancelled')),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для заказов
CREATE INDEX idx_orders_customer ON orders (customer_id);
CREATE INDEX idx_orders_branch ON orders (branch_id);
CREATE INDEX idx_orders_status ON orders (status);
CREATE INDEX idx_orders_delivery_date ON orders (delivery_date);

-- Создание таблицы складских позиций
CREATE TABLE IF NOT EXISTS positions (
    position_id SERIAL PRIMARY KEY,
    branch_id INT NOT NULL REFERENCES branches(branch_id),
    status VARCHAR(20) NOT NULL CHECK (status IN ('Active', 'Inactive', 'Maintenance')),
    zone_code VARCHAR(10) NOT NULL,
    first_level_storage_type VARCHAR(30) NOT NULL,
    fls_number VARCHAR(20) NOT NULL,
    second_level_storage VARCHAR(30),
    third_level_storage VARCHAR(30),
    length DOUBLE PRECISION CHECK (length > 0),
    width DOUBLE PRECISION CHECK (width > 0),
    height DOUBLE PRECISION CHECK (height > 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Уникальный индекс для кодов позиций
CREATE INDEX idx_positions_branch ON positions (branch_id);
CREATE INDEX idx_positions_status ON positions (status);
CREATE INDEX idx_positions_zone ON positions (zone_code);

-- Создание таблицы позиций заказа
CREATE TABLE IF NOT EXISTS order_positions (
    unique_id SERIAL PRIMARY KEY,
    order_id INT NOT NULL REFERENCES orders(order_id),
    item_position_id INT NOT NULL REFERENCES positions(position_id),
    quantity INT NOT NULL CHECK (quantity > 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индекс для быстрого поиска по заказам
CREATE INDEX idx_order_positions_order ON order_positions (order_id);
CREATE INDEX idx_order_positions_item ON order_positions (item_position_id);

-- Создание таблицы позиций товара
CREATE TABLE IF NOT EXISTS item_positions (
    id SERIAL PRIMARY KEY,
    item_id INT NOT NULL REFERENCES items(item_id),
    position_id INT NOT NULL REFERENCES positions(position_id),
    quantity INT NOT NULL CHECK (quantity > 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для товарных позиций
CREATE INDEX idx_item_positions_position ON item_positions (position_id);
CREATE INDEX idx_item_positions_item ON item_positions (item_id);
CREATE UNIQUE INDEX idx_item_positions_id ON item_positions (id);

-- Создание таблицы перемещений товара
CREATE TABLE IF NOT EXISTS item_movements (
    id SERIAL PRIMARY KEY,
    source_item_position_id INT REFERENCES item_positions(id),
    destination_position_id INT REFERENCES positions(position_id),
    source_branch_id INT REFERENCES branches(branch_id),
    destination_branch_id INT REFERENCES branches(branch_id),
    quantity INT NOT NULL CHECK (quantity > 0),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Индексы для перемещений
CREATE INDEX idx_item_movements_source_itempos ON item_movements (source_item_position_id);
CREATE INDEX idx_item_movements_destination_pos ON item_movements (destination_position_id);
CREATE INDEX idx_item_movements_source_branch ON item_movements (source_branch_id);
CREATE INDEX idx_item_movements_destination_branch ON item_movements (destination_branch_id);
-- Создание таблицы статусов товара
CREATE TABLE IF NOT EXISTS item_statuses (
    id SERIAL PRIMARY KEY,
    item_position_id INT NOT NULL REFERENCES item_positions(id),
    status VARCHAR(20) NOT NULL CHECK (status IN ('Available', 'Reserved', 'Shipped', 'Defective')),
    status_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    quantity INT NOT NULL CHECK (quantity > 0)
);

-- Индексы для статусов
CREATE INDEX idx_item_statuses_status ON item_statuses (status);
CREATE INDEX idx_item_statuses_position ON item_statuses (item_position_id);
CREATE INDEX idx_item_statuses_date ON item_statuses (status_date);


