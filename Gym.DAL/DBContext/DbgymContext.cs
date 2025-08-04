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

    public virtual DbSet<Contenido> Contenidos { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<EntrenadorCliente> EntrenadorClientes { get; set; }

    public virtual DbSet<Entrenadores> Entrenadores { get; set; }

    public virtual DbSet<HistorialAsistencia> HistorialAsistencias { get; set; }

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
            entity.HasKey(e => e.AsignacionId).HasName("PK__Asignaci__D82B5BB75E2FEDD8");

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
                .HasConstraintName("FK__Asignacio__idMem__37A5467C");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AsignacionesMembresia)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Asignacio__idUsu__36B12243");
        });

        modelBuilder.Entity<AsignacionesPlanEntrenamiento>(entity =>
        {
            entity.HasKey(e => e.AsignacionId).HasName("PK__Asignaci__D82B5BB76B973CAD");

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
                .HasConstraintName("FK__Asignacio__Clien__5441852A");

            entity.HasOne(d => d.Plan).WithMany(p => p.AsignacionesPlanEntrenamientos)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__Asignacio__PlanI__5535A963");
        });

        modelBuilder.Entity<AsistenciaPersonal>(entity =>
        {
            entity.HasKey(e => e.AsistenciaId).HasName("PK__Asistenc__72710F45A602A51F");

            entity.ToTable("AsistenciaPersonal");

            entity.Property(e => e.AsistenciaId).HasColumnName("AsistenciaID");
            entity.Property(e => e.FechaAsistencia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AsistenciaPersonals)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Asistenci__idUsu__3A81B327");
        });

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.IdCaja).HasName("PK__Caja__8BC79B34453FC576");

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
                .HasConstraintName("FK__Caja__idUsuario__59FA5E80");
        });

        modelBuilder.Entity<Contenido>(entity =>
        {
            entity.HasKey(e => e.IdContenido).HasName("PK__Contenid__7FB5C29EE2506790");

            entity.ToTable("Contenido");

            entity.Property(e => e.IdContenido).HasColumnName("idContenido");
            entity.Property(e => e.Comentarios)
                .IsUnicode(false)
                .HasColumnName("comentarios");
            entity.Property(e => e.Imagenes).HasColumnName("imagenes");
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
            entity.HasKey(e => e.IdEmpresa).HasName("PK__Empresa__F4BB6039C4704124");

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
            entity.HasKey(e => e.AsignacionId).HasName("PK__Entrenad__D82B5BB7421435D1");

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
                .HasConstraintName("FK__Entrenado__Clien__49C3F6B7");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.EntrenadorClienteEntrenadors)
                .HasForeignKey(d => d.EntrenadorId)
                .HasConstraintName("FK__Entrenado__Entre__4AB81AF0");
        });

        modelBuilder.Entity<Entrenadores>(entity =>
        {
            entity.HasKey(e => e.EntrenadorId).HasName("PK__Entrenad__D0EE85454237122D");

            entity.Property(e => e.EntrenadorId).HasColumnName("EntrenadorID");
            entity.Property(e => e.Especialidad).IsUnicode(false);
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Entrenadores)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Entrenado__idUsu__4222D4EF");
        });

        modelBuilder.Entity<HistorialAsistencia>(entity =>
        {
            entity.HasKey(e => e.HistorialId).HasName("PK__Historia__975206EF3414AE62");

            entity.Property(e => e.HistorialId).HasColumnName("HistorialID");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaAsistencia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialAsistencia)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Historial__idUsu__45F365D3");
        });

        modelBuilder.Entity<HistorialPago>(entity =>
        {
            entity.HasKey(e => e.HistorialPagoId).HasName("PK__Historia__1EEFBB82C43BB456");

            entity.Property(e => e.HistorialPagoId).HasColumnName("HistorialPagoID");
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PagoId).HasColumnName("PagoID");
            entity.Property(e => e.TipoPago).HasMaxLength(50);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Historial__idUsu__4E88ABD4");

            entity.HasOne(d => d.Pago).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.PagoId)
                .HasConstraintName("FK__Historial__PagoI__4D94879B");
        });

        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.HasKey(e => e.IdMembresia).HasName("PK__Membresi__BDDB80E9B2CCF76C");

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
            entity.HasKey(e => e.IdMenu).HasName("PK__Menu__C26AF4835E07CBB8");

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
            entity.HasKey(e => e.IdMenuRol).HasName("PK__MenuRol__9D6D61A45831DD4E");

            entity.ToTable("MenuRol");

            entity.Property(e => e.IdMenuRol).HasColumnName("idMenuRol");
            entity.Property(e => e.IdMenu).HasColumnName("idMenu");
            entity.Property(e => e.IdRol).HasColumnName("idRol");

            entity.HasOne(d => d.IdMenuNavigation).WithMany(p => p.MenuRols)
                .HasForeignKey(d => d.IdMenu)
                .HasConstraintName("FK__MenuRol__idMenu__29572725");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.MenuRols)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK__MenuRol__idRol__2A4B4B5E");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.PagoId).HasName("PK__Pagos__F00B6158BE1D97C6");

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
                .HasConstraintName("FK__Pagos__idUsuario__3E52440B");
        });

        modelBuilder.Entity<PlanesDeEntrenamiento>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__PlanesDe__755C22D70660CAA6");

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
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__3C872F7672B2E593");

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
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A6713CD8B3");

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
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreCompleto");
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
                .HasConstraintName("FK__Usuario__idRol__2D27B809");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
