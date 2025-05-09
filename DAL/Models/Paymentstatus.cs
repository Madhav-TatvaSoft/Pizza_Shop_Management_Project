﻿using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Paymentstatus
{
    public long PaymentStatusId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
