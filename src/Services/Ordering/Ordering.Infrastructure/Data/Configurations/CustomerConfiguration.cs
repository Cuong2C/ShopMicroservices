using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        // define conversion for CustomerId value object
        builder.Property(c => c.Id).HasConversion(
            customerId => customerId.Value, // Id has type CustomerId => get value and map to database
            dbId => CustomerId.Of(dbId)); // Get Id from database and convert to CustomerId

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();

        builder.Property(c => c.Email).HasMaxLength(255);
        builder.HasIndex(c => c.Email).IsUnique();
    }
}
