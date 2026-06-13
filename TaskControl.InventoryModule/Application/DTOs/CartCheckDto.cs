using System;
using System.Collections.Generic;
using TaskControl.InformationModule.Application.DTOs;

namespace TaskControl.InventoryModule.Application.DTOs
{
    /// <summary>
    /// Представляет товар в корзине при проверке наличия.
    /// </summary>
    public class CartItemDto
    {
        /// <summary>
        /// Уникальный идентификатор товара.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Требуемое количество товара.
        /// </summary>
        public int RequiredQuantity { get; set; }
    }

    /// <summary>
    /// Запрос для проверки наличия всей корзины.
    /// </summary>
    public class CartCheckRequestDto
    {
        /// <summary>
        /// Список товаров в корзине.
        /// </summary>
        public List<CartItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Описание товара, которого не хватает в филиале.
    /// </summary>
    public class MissingItemDto
    {
        /// <summary>
        /// Идентификатор товара.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Требуемое количество товара.
        /// </summary>
        public int RequiredQuantity { get; set; }

        /// <summary>
        /// Фактически доступное количество товара.
        /// </summary>
        public int AvailableQuantity { get; set; }
    }

    /// <summary>
    /// Сведения о филиале с частичным наличием товаров из корзины.
    /// </summary>
    public class BranchAvailabilityDto
    {
        /// <summary>
        /// Данные филиала.
        /// </summary>
        public BranchDto Branch { get; set; } = null!;

        /// <summary>
        /// Список недостающих товаров.
        /// </summary>
        public List<MissingItemDto> MissingItems { get; set; } = new();
    }

    /// <summary>
    /// Ответ с результатами проверки наличия корзины по всем филиалам.
    /// </summary>
    public class BranchAvailabilityResponseDto
    {
        /// <summary>
        /// Филиалы, в которых есть все товары в нужном количестве.
        /// </summary>
        public List<BranchDto> AvailableBranches { get; set; } = new();

        /// <summary>
        /// Филиалы, в которых товары есть частично.
        /// </summary>
        public List<BranchAvailabilityDto> PartiallyAvailableBranches { get; set; } = new();
    }
}
