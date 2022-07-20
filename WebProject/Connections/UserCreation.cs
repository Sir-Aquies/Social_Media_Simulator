using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebProject.Data;

namespace WebProject.Connections
{
    [DbContext(typeof(WebProjectSQL))]
    partial class UserCreation : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("WebProject.Models.UserModel", b =>
            {
                b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                //b.Property<string>("Genre")
                //.IsRequired()

            });
        }
    }
}
