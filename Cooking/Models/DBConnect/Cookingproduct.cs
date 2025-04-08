﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect
{
    [Table("Cookingproduct")]
    public class Cookingproduct
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string? image { get; set; }
        public double price { get; set; }
        public int size { get; set; }
        public int quantity { get; set; }
        public override string ToString()
        {
            return $"ID: {id}, Name: {name}, Image: {image}, Price: {price}, Size: {size}, Quantity: {quantity}";
        }

    }

}