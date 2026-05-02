# API Migration: legacy endpoints -> WorkerTasksController

Переходный период: legacy-эндпоинты остаются доступными, помечены как deprecated, и возвращают warning-заголовки:
- `Warning: 299 - Deprecated API...`
- `X-Api-Deprecated: true`

## Таблица соответствия URL

| Legacy endpoint | Unified endpoint |
|---|---|
| `GET /api/v1/Inventory/worker/{userId}/assignments` | `GET /api/v1/WorkerTasks/{workerId}/pending` |
| `GET /api/v1/Inventory/assignment/{id}/details` | `GET /api/v1/WorkerTasks/{taskId}/details` |
| `POST /api/v1/Inventory/assignment/{id}/start` | `POST /api/v1/WorkerTasks/{taskId}/start?workerId=...` |
| `POST /api/v1/Inventory/assignment/{id}/pause` | `POST /api/v1/WorkerTasks/{taskId}/pause` |
| `POST /api/v1/Inventory/assignment/{id}/cancel` | `POST /api/v1/WorkerTasks/{taskId}/cancel` |
| `POST /api/v1/Inventory/complete-assignment` | `POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...` |
| `GET /api/v1/OrderAssembly/worker/{userId}/assignments` | `GET /api/v1/WorkerTasks/{workerId}/pending` |
| `GET /api/v1/OrderAssembly/assignment/{id}/details` | `GET /api/v1/WorkerTasks/{taskId}/details` |
| `POST /api/v1/OrderAssembly/assignment/{id}/start` | `POST /api/v1/WorkerTasks/{taskId}/start?workerId=...` |
| `POST /api/v1/OrderAssembly/assignment/{id}/pause` | `POST /api/v1/WorkerTasks/{taskId}/pause` |
| `POST /api/v1/OrderAssembly/assignment/{id}/cancel` | `POST /api/v1/WorkerTasks/{taskId}/cancel` |
| `POST /api/v1/OrderAssembly/complete/{assignmentId}` | `POST /api/v1/WorkerTasks/{taskId}/complete?workerId=...` |

## План удаления legacy endpoints

1. Мобильный клиент полностью переводится на `WorkerTasksController`.
2. Выпускается отдельный релиз, удаляющий legacy-endpoints из `InventoryController` и `OrderAssemblyController`.
