

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


-- 1. Вставка филиалов
INSERT INTO branches (branch_name, branch_type, address)
VALUES 
    ('Центральный склад', 'Warehouse', 'г. Москва, ул. Ленина, 1'),
    ('Филиал Восток', 'Retail', 'г. Владивосток, ул. Портовая, 42'),
    ('Филиал Запад', 'Distribution', 'г. Калининград, пр. Мира, 15');

-- 2. Вставка сотрудников
INSERT INTO employees (surname, name, middle_name, role)
VALUES
    ('Иванов', 'Иван', 'Иванович', 'Кладовщик'),
    ('Петрова', 'Мария', 'Сергеевна', 'Логист'),
    ('Сидоров', 'Алексей', NULL, 'Грузчик');

-- 3. Вставка товаров
INSERT INTO items (weight, length, width, height)
VALUES
    (1.5, 20, 15, 10),
    (0.8, 10, 10, 5),
    (5.0, 50, 30, 20);

-- 4. Вставка складских позиций
INSERT INTO positions (
    branch_id, status, zone_code, first_level_storage_type, fls_number,
    second_level_storage, third_level_storage, length, width, height
)
VALUES
    (1, 'Active', 'A', 'Стеллаж', 'A-01', 'Полка 2', 'Ячейка 3', 100, 50, 40),
    (1, 'Active', 'B', 'Паллет', 'B-05', NULL, NULL, 120, 80, 100),
    (2, 'Active', 'C', 'Контейнер', 'C-12', 'Секция 1', NULL, 60, 40, 30);

-- 5. Вставка товарных позиций
INSERT INTO item_positions (item_id, position_id, quantity)
VALUES
    (1, 1, 10),
    (2, 2, 5),
    (3, 3, 2);

-- 6. Вставка заказов
INSERT INTO orders (customer_id, branch_id, delivery_date, type, status)
VALUES
    (1001, 1, '2025-07-20 14:00:00', 'Online', 'Processing'),
    (1002, 2, '2025-07-22 10:00:00', 'Offline', 'New'),
    (1003, 1, '2025-07-25 16:00:00', 'Wholesale', 'Shipped');

-- 7. Вставка позиций заказа
INSERT INTO order_positions (order_id, item_position_id, quantity)
VALUES
    (1, 1, 2),
    (1, 2, 1),
    (2, 3, 3);

-- 8. Вставка отметок сотрудников
INSERT INTO check_io_employees (employee_id, branch_id, check_type, check_timestamp)
VALUES
    (1, 1, 'in', '2025-07-16 08:00:00'),
    (1, 1, 'out', '2025-07-16 17:30:00'),
    (2, 2, 'in', '2025-07-16 09:15:00');

-- 9. Вставка сырых событий
INSERT INTO raw_events (type, json_params, event_time, source_service)
VALUES
    (
        'scan', 
        '{"barcode": "123456", "location": "A-01"}', 
        '2025-07-16 10:30:00', 
        'ScannerService'
    ),
    (
        'login', 
        '{"user_id": 2, "device": "tablet"}', 
        '2025-07-16 09:00:00', 
        'AuthService'
    );

-- 10. Вставка активных задач
INSERT INTO active_tasks (branch_id, type, status, json_params)
VALUES
    (1, 'Переучет', 'InProgress', '{"zone": "A"}'),
    (2, 'Приемка', 'New', '{"supplier": "ООО Поставщик"}'),
    (1, 'Комплектация', 'Completed', '{"order_id": 1}');

-- 11. Вставка назначенных задач
INSERT INTO active_assigned_tasks (task_id, user_id, assigned_at)
VALUES
    (1, 1, '2025-07-16 08:30:00'),
    (2, 2, '2025-07-16 09:45:00'),
    (3, 1, '2025-07-15 14:20:00');

-- 12. Вставка перемещений товара
INSERT INTO item_movements (
    source_item_position_id, destination_position_id, 
    source_branch_id, destination_branch_id, quantity
)
VALUES
    (1, 2, 1, 1, 5),
    (3, 1, 2, 1, 2);



    -- Дополнительные отметки сотрудников (10 записей)
INSERT INTO check_io_employees (employee_id, branch_id, check_type, check_timestamp)
VALUES
    (2, 2, 'out', '2025-07-16 18:30:00'),
    (3, 1, 'in', '2025-07-16 07:45:00'),
    (3, 1, 'out', '2025-07-16 16:20:00'),
    (1, 1, 'in', '2025-07-17 08:05:00'),
    (1, 1, 'out', '2025-07-17 17:40:00'),
    (2, 2, 'in', '2025-07-17 09:20:00'),
    (2, 2, 'out', '2025-07-17 18:15:00'),
    (3, 1, 'in', '2025-07-17 07:50:00'),
    (3, 1, 'out', '2025-07-17 16:35:00'),
    (1, 1, 'in', '2025-07-18 08:10:00');

-- Дополнительные сырые события (8 записей)
INSERT INTO raw_events (type, json_params, event_time, source_service)
VALUES
    ('scan', '{"barcode": "789012", "location": "B-05"}', '2025-07-16 11:15:00', 'ScannerService'),
    ('logout', '{"user_id": 1, "device": "handheld"}', '2025-07-16 17:35:00', 'AuthService'),
    ('error', '{"code": "E404", "message": "Не найдено"}', '2025-07-17 10:20:00', 'InventoryService'),
    ('update', '{"position": "A-01", "quantity": 15}', '2025-07-17 14:00:00', 'WMS'),
    ('scan', '{"barcode": "345678", "location": "C-12"}', '2025-07-18 09:45:00', 'MobileApp'),
    ('login', '{"user_id": 3, "device": "desktop"}', '2025-07-18 08:30:00', 'AuthService'),
    ('movement', '{"from": "A-01", "to": "B-05", "items": 5}', '2025-07-18 11:20:00', 'WMS'),
    ('alert', '{"type": "low_stock", "position": "C-12"}', '2025-07-18 15:40:00', 'Monitoring');

-- Дополнительные активные задачи (8 записей)
INSERT INTO active_tasks (branch_id, type, status, json_params, created_at, completed_at)
VALUES
    (1, 'Инвентаризация', 'New', '{"zone": "B"}', '2025-07-17 09:00:00', NULL),
    (2, 'Перемещение', 'InProgress', '{"target_branch": 1}', '2025-07-17 10:30:00', NULL),
    (3, 'Приемка', 'New', '{"supplier": "ООО Снабжение"}', '2025-07-17 11:45:00', NULL),
    (1, 'Упаковка', 'Completed', '{"order_id": 3}', '2025-07-16 13:20:00', '2025-07-16 15:40:00'),
    (2, 'Выгрузка', 'InProgress', '{"truck": "A123BC"}', '2025-07-18 08:15:00', NULL),
    (1, 'Маркировка', 'New', '{"items_count": 50}', '2025-07-18 10:00:00', NULL),
    (3, 'Резервирование', 'Completed', '{"order_id": 2}', '2025-07-17 14:30:00', '2025-07-17 16:00:00'),
    (2, 'Комплектация', 'InProgress', '{"order_id": 5}', '2025-07-18 09:30:00', NULL);

-- Дополнительные назначения задач (8 записей)
INSERT INTO active_assigned_tasks (task_id, user_id, assigned_at)
VALUES
    (4, 2, '2025-07-16 10:00:00'),
    (5, 3, '2025-07-17 11:00:00'),
    (6, 1, '2025-07-17 12:30:00'),
    (7, 2, '2025-07-18 08:20:00'),
    (8, 3, '2025-07-18 09:35:00'),
    (9, 1, '2025-07-18 10:05:00'),
    (10, 2, '2025-07-18 11:15:00'),
    (11, 3, '2025-07-18 13:40:00');

-- Дополнительные заказы (8 записей)
INSERT INTO orders (customer_id, branch_id, delivery_date, type, status)
VALUES
    (1004, 3, '2025-07-23 12:00:00', 'Online', 'Processing'),
    (1005, 1, '2025-07-24 15:30:00', 'Wholesale', 'New'),
    (1006, 2, '2025-07-25 11:00:00', 'Offline', 'Shipped'),
    (1007, 1, '2025-07-26 14:45:00', 'Online', 'Delivered'),
    (1008, 3, '2025-07-27 10:15:00', 'Online', 'Cancelled'),
    (1009, 2, '2025-07-28 16:20:00', 'Wholesale', 'Processing'),
    (1010, 1, '2025-07-29 09:30:00', 'Offline', 'New'),
    (1011, 3, '2025-07-30 13:00:00', 'Online', 'Processing');

-- Дополнительные позиции заказов (10 записей)
INSERT INTO order_positions (order_id, item_position_id, quantity)
VALUES
    (4, 1, 3),
    (4, 3, 1),
    (5, 2, 4),
    (6, 1, 2),
    (6, 2, 2),
    (7, 3, 1),
    (8, 1, 5),
    (9, 2, 3),
    (10, 3, 2),
    (11, 1, 4);

-- Дополнительные товарные позиции (8 записей)
INSERT INTO item_positions (item_id, position_id, quantity)
VALUES
    (1, 2, 8),
    (2, 3, 6),
    (3, 1, 3),
    (1, 3, 4),
    (2, 1, 7),
    (3, 2, 2),
    (1, 1, 5),
    (2, 2, 9);

-- Дополнительные перемещения товара (8 записей)
INSERT INTO item_movements (
    source_item_position_id, destination_position_id, 
    source_branch_id, destination_branch_id, quantity
)
VALUES
    (2, 1, 1, 1, 3),
    (4, 3, 1, 1, 2),
    (1, 2, 1, 1, 4),
    (3, 1, 1, 1, 1),
    (5, 3, 1, 1, 2),
    (6, 2, 1, 1, 3),
    (7, 1, 1, 1, 2),
    (8, 3, 1, 1, 4);

-- 13. Вставка статусов товара
INSERT INTO item_statuses (item_position_id, status, quantity)
VALUES
    (1, 'Reserved', 3),
    (2, 'Available', 5),
    (3, 'Shipped', 2),
    (4, 'Reserved', 2),
    (5, 'Available', 7),
    (6, 'Shipped', 1),
    (7, 'Available', 5),
    (8, 'Reserved', 3),
    (9, 'Defective', 1),
    (10, 'Available', 4);

-- =====================================================
-- ТАБЛИЦЫ ДЛЯ ИНВЕНТАРИЗАЦИИ
-- =====================================================

-- Создание таблицы назначений инвентаризации
CREATE TABLE IF NOT EXISTS inventory_assignments (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL REFERENCES active_tasks(task_id),
    assigned_to_user_id INT NOT NULL REFERENCES employees(employees_id),
    branch_id INT NOT NULL REFERENCES branches(branch_id),
    zone_code VARCHAR(10),
    status INT NOT NULL CHECK (status IN (0, 1, 2, 3)), -- Assigned(0), InProgress(1), Completed(2), Cancelled(3)
    assigned_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP
);

-- Индексы для таблицы inventory_assignments
CREATE INDEX idx_inventory_assignments_task ON inventory_assignments(task_id);
CREATE INDEX idx_inventory_assignments_user ON inventory_assignments(assigned_to_user_id);
CREATE INDEX idx_inventory_assignments_branch ON inventory_assignments(branch_id);
CREATE INDEX idx_inventory_assignments_zone ON inventory_assignments(zone_code);
CREATE INDEX idx_inventory_assignments_status ON inventory_assignments(status);

-- Создание таблицы строк инвентаризации
CREATE TABLE IF NOT EXISTS inventory_assignment_lines (
    id SERIAL PRIMARY KEY,
    inventory_assignment_id INT NOT NULL REFERENCES inventory_assignments(id),
    item_position_id INT NOT NULL REFERENCES item_positions(id),
    position_id INT NOT NULL REFERENCES positions(position_id),
    expected_quantity INT NOT NULL CHECK (expected_quantity >= 0),
    actual_quantity INT,
    zone_code VARCHAR(10) NOT NULL,
    first_level_storage_type VARCHAR(30) NOT NULL,
    fls_number VARCHAR(20) NOT NULL,
    second_level_storage VARCHAR(30),
    third_level_storage VARCHAR(30)
);

-- Индексы для таблицы inventory_assignment_lines
CREATE INDEX idx_inventory_lines_assignment ON inventory_assignment_lines(inventory_assignment_id);
CREATE INDEX idx_inventory_lines_itemposition ON inventory_assignment_lines(item_position_id);
CREATE INDEX idx_inventory_lines_position ON inventory_assignment_lines(position_id);
CREATE INDEX idx_inventory_lines_zone ON inventory_assignment_lines(zone_code);

-- Создание таблицы расхождений при инвентаризации
CREATE TABLE IF NOT EXISTS inventory_discrepancies (
    id SERIAL PRIMARY KEY,
    inventory_assignment_line_id INT NOT NULL REFERENCES inventory_assignment_lines(id),
    item_position_id INT NOT NULL REFERENCES item_positions(id),
    expected_quantity INT NOT NULL CHECK (expected_quantity >= 0),
    actual_quantity INT NOT NULL CHECK (actual_quantity >= 0),
    type INT NOT NULL CHECK (type IN (0, 1, 2)), -- None(0), Surplus(1), Shortage(2)
    note TEXT,
    identified_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    resolution_status INT NOT NULL CHECK (resolution_status IN (0, 1, 2, 3)), -- Pending(0), Resolved(1), UnderInvestigation(2), WrittenOff(3)
    CONSTRAINT positive_variance CHECK ((actual_quantity - expected_quantity) IS NOT NULL)
);

-- Индексы для таблицы inventory_discrepancies
CREATE INDEX idx_discrepancies_line ON inventory_discrepancies(inventory_assignment_line_id);
CREATE INDEX idx_discrepancies_itemposition ON inventory_discrepancies(item_position_id);
CREATE INDEX idx_discrepancies_type ON inventory_discrepancies(type);
CREATE INDEX idx_discrepancies_resolution ON inventory_discrepancies(resolution_status);
CREATE INDEX idx_discrepancies_identified ON inventory_discrepancies(identified_at);

-- Создание таблицы статистики инвентаризации
CREATE TABLE IF NOT EXISTS inventory_statistics (
    id SERIAL PRIMARY KEY,
    inventory_assignment_id INT NOT NULL UNIQUE REFERENCES inventory_assignments(id),
    total_positions INT NOT NULL CHECK (total_positions > 0),
    counted_positions INT NOT NULL CHECK (counted_positions >= 0),
    discrepancy_count INT NOT NULL CHECK (discrepancy_count >= 0),
    surplus_count INT NOT NULL CHECK (surplus_count >= 0),
    shortage_count INT NOT NULL CHECK (shortage_count >= 0),
    total_surplus_quantity INT NOT NULL CHECK (total_surplus_quantity >= 0),
    total_shortage_quantity INT NOT NULL CHECK (total_shortage_quantity >= 0),
    started_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP
);

-- Индексы для таблицы inventory_statistics
CREATE INDEX idx_statistics_assignment ON inventory_statistics(inventory_assignment_id);
CREATE INDEX idx_statistics_started ON inventory_statistics(started_at);
CREATE INDEX idx_statistics_completed ON inventory_statistics(completed_at);

