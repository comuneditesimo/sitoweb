using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class PriceListItem
    {
        private string _description = "";
        private double? _quantity { get; set; } = 0.0;
        private double _price { get; set; } = 0.0;

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }
        public double? Quantity
        {
            get
            {
                return this._quantity;
            }
            set
            {
                if (value >= 0.0)
                {
                    this._quantity = value;
                }
            }
        }
        public double Price
        {
            get
            {
                return this._price;
            }
            set
            {
                if (value >= 0.0)
                {
                    this._price = value;
                }
            }
        }

        public PriceListItem(string description, double price, double? quantity = null)
        {
            Description = description;
            Price = price;
            Quantity = quantity;
        }
    }
}
