﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Mystique.Core.ViewModels
{
    public class PluginViewModel
    {
        public Guid PluginId { get; set; }
        public string UniqueKey { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Version { get; set; }
        public bool IsEnable { get; set; }
    }

    public class PluginViewModelConfiguration : IEntityTypeConfiguration<PluginViewModel>
    {
        public void Configure(EntityTypeBuilder<PluginViewModel> builder)
        {
            builder.Property(o => o.PluginId).HasMaxLength(64);
            builder.Property(o => o.UniqueKey).HasMaxLength(64);
            builder.Property(o => o.Name).HasMaxLength(64);
            builder.Property(o => o.DisplayName).HasMaxLength(short.MaxValue);
            builder.Property(o => o.Version).HasMaxLength(32);

            builder.ToTable("Plugins");
            builder.HasKey(o => o.PluginId);
        }
    }

    public class FtpFileDetail
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
    }

    public class FtpFileDetailConfiguration : IEntityTypeConfiguration<FtpFileDetail>
    {
        public void Configure(EntityTypeBuilder<FtpFileDetail> builder)
        {
            builder.Property(o => o.Name).HasMaxLength(64);

            builder.ToTable("FtpFileDetails");
            builder.HasKey(o => o.Name);
            builder.Property(o => o.Name).HasMaxLength(255);
        }
    }
}
