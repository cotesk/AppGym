using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Gym.Model;

public partial class DbgymContext : DbContext
{
    public DbgymContext()
    {
    }

    public DbgymContext(DbContextOptions<DbgymContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsignacionesMembresia> AsignacionesMembresia { get; set; }

    public virtual DbSet<AsignacionesPlanEntrenamiento> AsignacionesPlanEntrenamientos { get; set; }

    public virtual DbSet<AsistenciaPersonal> AsistenciaPersonals { get; set; }

    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<CodigoActivacion> CodigoActivacions { get; set; }

    public virtual DbSet<Contenido> Contenidos { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<EntrenadorCliente> EntrenadorClientes { get; set; }

    public virtual DbSet<HistorialPago> HistorialPagos { get; set; }

    public virtual DbSet<Membresia> Membresia { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuRol> MenuRols { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<PlanesDeEntrenamiento> PlanesDeEntrenamientos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsignacionesMembresia>(entity =>
        {
            entity.HasKey(e => e.AsignacionId).HasName("PK__Asignaci__D82B5BB7032CDAC2");

            entity.Property(e => e.AsignacionId).HasColumnName("AsignacionID");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaVencimiento).HasColumnType("datetime");
            entity.Property(e => e.IdMembresia).HasColumnName("idMembresia");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdMembresiaNavigation).WithMany(p => p.AsignacionesMembresia)
                .HasForeignKey(d => d.IdMembresia)
                .HasConstraintName("FK__Asignacio__idMem__4AB81AF0");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AsignacionesMembresia)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Asignacio__idUsu__49C3F6B7");
        });

        modelBuilder.Entity<AsignacionesPlanEntrenamiento>(entity =>
        {
            entity.HasKey(e => e.AsignacionId).HasName("PK__Asignaci__D82B5BB7C0A4B4E6");

            entity.ToTable("AsignacionesPlanEntrenamiento");

            entity.Property(e => e.AsignacionId).HasColumnName("AsignacionID");
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaFinAsignacion).HasColumnType("datetime");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");

            entity.HasOne(d => d.Cliente).WithMany(p => p.AsignacionesPlanEntrenamientos)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK__Asignacio__Clien__619B8048");

            entity.HasOne(d => d.Plan).WithMany(p => p.AsignacionesPlanEntrenamientos)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__Asignacio__PlanI__628FA481");
        });

        modelBuilder.Entity<AsistenciaPersonal>(entity =>
        {
            entity.HasKey(e => e.AsistenciaId).HasName("PK__Asistenc__72710F45D1262C1D");

            entity.ToTable("AsistenciaPersonal");

            entity.Property(e => e.AsistenciaId).HasColumnName("AsistenciaID");
            entity.Property(e => e.FechaAsistencia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AsistenciaPersonals)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Asistenci__idUsu__4D94879B");
        });

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.IdCaja).HasName("PK__Caja__8BC79B34EECE731E");

            entity.ToTable("Caja");

            entity.Property(e => e.IdCaja).HasColumnName("idCaja");
            entity.Property(e => e.Comentarios)
                .IsUnicode(false)
                .HasColumnName("comentarios");
            entity.Property(e => e.ComentariosDevoluciones)
                .IsUnicode(false)
                .HasColumnName("comentariosDevoluciones");
            entity.Property(e => e.ComentariosGastos)
                .IsUnicode(false)
                .HasColumnName("comentariosGastos");
            entity.Property(e => e.Devoluciones)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("devoluciones");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaApertura)
                .HasColumnType("datetime")
                .HasColumnName("fechaApertura");
            entity.Property(e => e.FechaCierre)
                .HasColumnType("datetime")
                .HasColumnName("fechaCierre");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.Gastos)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("gastos");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Ingresos)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ingresos");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("metodoPago");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreUsuario");
            entity.Property(e => e.Prestamos)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("prestamos");
            entity.Property(e => e.SaldoFinal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("saldoFinal");
            entity.Property(e => e.SaldoInicial)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("saldoInicial");
            entity.Property(e => e.Transacciones)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("transacciones");
            entity.Property(e => e.UltimaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ultimaActualizacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Caja__idUsuario__6754599E");
        });

        modelBuilder.Entity<CodigoActivacion>(entity =>
        {
            entity.HasKey(e => e.IdCodigoActivacion).HasName("PK__CodigoAc__C105F9E97D261E9B");

            entity.ToTable("CodigoActivacion");

            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.FechaGeneracion).HasColumnType("datetime");
        });

        modelBuilder.Entity<Contenido>(entity =>
        {
            entity.HasKey(e => e.IdContenido).HasName("PK__Contenid__7FB5C29E66C4E033");

            entity.ToTable("Contenido");

            entity.Property(e => e.IdContenido).HasColumnName("idContenido");
            entity.Property(e => e.Comentarios)
                .IsUnicode(false)
                .HasColumnName("comentarios");
            entity.Property(e => e.ImagenUrl)
                .IsUnicode(false)
                .HasColumnName("imagenUrl");
            entity.Property(e => e.Imagenes).HasColumnName("imagenes");
            entity.Property(e => e.NombreImagen)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("nombreImagen");
            entity.Property(e => e.TipoComentarios)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("tipoComentarios");
            entity.Property(e => e.TipoContenido)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("tipoContenido");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("PK__Empresa__F4BB6039BD12BB11");

            entity.ToTable("Empresa");

            entity.Property(e => e.IdEmpresa)
                .ValueGeneratedNever()
                .HasColumnName("ID_Empresa");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Facebook)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("facebook");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.Instagram)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("instagram");
            entity.Property(e => e.LogoNombre)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("logoNombre");
            entity.Property(e => e.Nit)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("nit");
            entity.Property(e => e.NombreEmpresa)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Nombre_Empresa");
            entity.Property(e => e.Propietario)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Tiktok)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasColumnName("tiktok");
        });

        modelBuilder.Entity<EntrenadorCliente>(entity =>
        {
            entity.HasKey(e => e.AsignacionId).HasName("PK__Entrenad__D82B5BB713DFC2F6");

            entity.ToTable("EntrenadorCliente");

            entity.Property(e => e.AsignacionId).HasColumnName("AsignacionID");
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.EntrenadorId).HasColumnName("EntrenadorID");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaFinAsignacion).HasColumnType("datetime");

            entity.HasOne(d => d.Cliente).WithMany(p => p.EntrenadorClienteClientes)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK__Entrenado__Clien__571DF1D5");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.EntrenadorClienteEntrenadors)
                .HasForeignKey(d => d.EntrenadorId)
                .HasConstraintName("FK__Entrenado__Entre__5812160E");
        });

        modelBuilder.Entity<HistorialPago>(entity =>
        {
            entity.HasKey(e => e.HistorialPagoId).HasName("PK__Historia__1EEFBB8239C8C036");

            entity.Property(e => e.HistorialPagoId).HasColumnName("HistorialPagoID");
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.TipoPago).HasMaxLength(50);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Historial__idUsu__5BE2A6F2");

            entity.HasOne(d => d.Pago).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.PagoId)
                .HasConstraintName("FK__Historial__PagoI__5AEE82B9");
        });

        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.HasKey(e => e.IdMembresia).HasName("PK__Membresi__BDDB80E914198B6C");

            entity.Property(e => e.IdMembresia).HasColumnName("idMembresia");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.DuracionDias).HasColumnName("duracionDias");
            entity.Property(e => e.EsActivo)
                .HasDefaultValueSql("((1))")
                .HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.IdMenu).HasName("PK__Menu__C26AF483F697A9BD");

            entity.ToTable("Menu");

            entity.Property(e => e.IdMenu).HasColumnName("idMenu");
            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("icono");
            entity.Property(e => e.IdMenuPadre).HasColumnName("idMenuPadre");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Url)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("url");
        });

        modelBuilder.Entity<MenuRol>(entity =>
        {
            entity.HasKey(e => e.IdMenuRol).HasName("PK__MenuRol__9D6D61A46506C50F");

            entity.ToTable("MenuRol");

            entity.Property(e => e.IdMenuRol).HasColumnName("idMenuRol");
            entity.Property(e => e.IdMenu).HasColumnName("idMenu");
            entity.Property(e => e.IdRol).HasColumnName("idRol");

            entity.HasOne(d => d.IdMenuNavigation).WithMany(p => p.MenuRols)
                .HasForeignKey(d => d.IdMenu)
                .HasConstraintName("FK__MenuRol__idMenu__3C69FB99");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.MenuRols)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__MenuRol__idRol__3D5E1FD2");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK__Pagos__F00B6158713D5936");

            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.FechaPago)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observaciones).IsUnicode(false);
            entity.Property(e => e.TipoPago)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__idUsuario__52593CB8");
        });

        modelBuilder.Entity<PlanesDeEntrenamiento>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__PlanesDe__755C22D756BF5B54");

            entity.ToTable("PlanesDeEntrenamiento");

            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoPlan)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__3C872F76033D9416");

            entity.ToTable("Rol");

            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A6D086E044");

            entity.ToTable("Usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Cedula)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("cedula");
            entity.Property(e => e.Clave)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("clave");
            entity.Property(e => e.Correo)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.EsActivo)
                .HasDefaultValueSql("((1))")
                .HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.ImagenUrl)
                .IsUnicode(false)
                .HasColumnName("imagenUrl");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreCompleto");
            entity.Property(e => e.NombreImagen)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("nombreImagen");
            entity.Property(e => e.RefreshToken)
                .IsUnicode(false)
                .HasColumnName("refreshToken");
            entity.Property(e => e.RefreshTokenExpiry)
                .HasColumnType("datetime")
                .HasColumnName("refreshTokenExpiry");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__Usuario__idRol__403A8C7D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
