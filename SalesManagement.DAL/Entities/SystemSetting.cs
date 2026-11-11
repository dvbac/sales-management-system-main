using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesManagement.DAL.Entities;

public partial class SystemSetting
{
    [Key]
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? GroupName { get; set; }

    public string? Description { get; set; }
}
