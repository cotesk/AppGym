import { Component, ViewChild } from '@angular/core';
import { AsitenciaPersonal } from '../../../../Interfaces/asistenciaPersonal';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import Swal from 'sweetalert2';
import { AsistenciaPersonalService } from '../../../../Services/asistencia.service';
import { ModalAsistenciaComponent } from '../../Modales/modal-asistencia/modal-asistencia.component';
import { CalendarioModalComponent } from '../../Modales/calendario-modal/calendario-modal.component';

@Component({
  selector: 'app-asistencia',
  templateUrl: './asistencia.component.html',
  styleUrl: './asistencia.component.css'
})
export class AsistenciaComponent {




  columnasTabla: string[] = ['asistenciaId','nombreUsuario','fechaAsistencia', 'pagoRealizado', 'acciones' ];
  dataInicio: AsitenciaPersonal[] = [];
  dataListaAsistencia = new MatTableDataSource(this.dataInicio);
  @ViewChild(MatPaginator) paginacionTabla!: MatPaginator;
  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  @ViewChild(MatSort) sort!: MatSort;
  selectedFile: File | null = null;


  page = 1;
  pageSize = 5;
  totalUsuario = 0;
  totalPages = 0;
  searchTerm = '';


  constructor(
    private dialog: MatDialog,
    private _asistenciaServicio: AsistenciaPersonalService,
    private _utilidadServicio: UtilidadService,
    private _usuarioServicio: UsuariosService,

  ) { }



   obtenerAsistencia() {

    this._asistenciaServicio.listaPaginada(this.page, this.pageSize, this.searchTerm).subscribe({
      next: (data) => {
        if (data && data.data && data.data.length > 0) {

          console.log('data :'+data.data)

          this.totalUsuario = data.total;
          this.totalPages = data.totalPages;
          // this.dataListaUsuarios.data = data.data;
          this.dataListaAsistencia.data = data.data;
        } else {
          this.totalUsuario = 0; // Reinicia el total de categorías si no hay datos
          this.totalPages = 0; // Reinicia el total de páginas si no hay datos
          this.dataListaAsistencia.data = []; // Limpia los datos existentes
          // Swal.fire({
          //   icon: 'warning',
          //   title: 'Advertencia',
          //   text: 'No se encontraron datos',
          // });
        }
      },
      error: (error) => this.handleTokenError(() => this.obtenerAsistencia())
    });

  }

  handleTokenError(retryCallback: () => void): void {
    this.totalUsuario = 0;
    this.totalPages = 0;
    this.dataListaAsistencia.data = [];
    const usuarioString = localStorage.getItem('usuario');
    if (usuarioString) {
      const bytes = CryptoJS.AES.decrypt(usuarioString, this.CLAVE_SECRETA);
      const datosDesencriptados = bytes.toString(CryptoJS.enc.Utf8);
      if (datosDesencriptados) {
        const usuario = JSON.parse(datosDesencriptados);
        this._usuarioServicio.obtenerUsuarioPorId(usuario.idUsuario).subscribe(
          (usuario: any) => {
            const refreshToken = usuario.refreshToken;
            this._usuarioServicio.renovarToken(refreshToken).subscribe(
              (response: any) => {
                localStorage.setItem('authToken', response.token);
                localStorage.setItem('refreshToken', response.refreshToken);
                retryCallback();
              },
              (error: any) => {
                // Manejar error de renovación de token
              }
            );
          },
          (error: any) => {
            // Manejar error al obtener usuario por ID
          }
        );
      }
    }
  }
  ngOnInit(): void {
    this.obtenerAsistencia();

  }


  ngAfterViewInit(): void {
    this.dataListaAsistencia.paginator = this.paginacionTabla;
  }


  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.obtenerAsistencia();
  }

  aplicarFiltroTabla(event: Event) {
    const filtroValue = (event.target as HTMLInputElement).value.trim().toLowerCase();

    if (filtroValue === 'pagado' || filtroValue === 'Pagado') {
      this.searchTerm = "true";
    } else if (filtroValue === 'no pagado' || filtroValue === 'No   Pagado') {
      this.searchTerm = "false";
    } else {
      this.searchTerm = filtroValue;
    }

    this.obtenerAsistencia();
  }
  firstPage() {
    this.page = 1;
    this.obtenerAsistencia();
  }

  previousPage() {
    if (this.page > 1) {
      this.page--;
      this.obtenerAsistencia();
    }
  }

  nextPage() {
    if (this.page < this.totalPages) {
      this.page++;
      this.obtenerAsistencia();
    }
  }

  lastPage() {
    this.page = this.totalPages;
    this.obtenerAsistencia();
  }
  pageSizeChange() {
    this.page = 1;
    this.obtenerAsistencia();
  }


  verCalendario(idUsuario: number): void {



    this._asistenciaServicio.consultarAsistenciasPorUsuario(idUsuario).subscribe(
      (response) => {
        if (response.status && response.value.length > 0) {
          const fechasRegistradas = response.value.map((asistencia: any) => {
            const fecha = asistencia.fechaAsistencia.split(' ')[0]; // Extraer solo la fecha (DD/MM/YYYY)
            return {
              fecha: fecha, // Mantener la fecha como cadena o convertirla a Moment según lo que necesites
              pagoRealizado: asistencia.pagoRealizado, // Añadir el estado de pago
              idUsuario: asistencia.idUsuario,
              asistenciaId:asistencia.asistenciaId
            };
          });
          // const fechasRegistradas = response.value.map((asistencia: any) => {
          //   const fecha = asistencia.fechaAsistencia.split(' ')[0]; // Extraer solo la fecha (DD/MM/YYYY)
          //   return fecha; // Mantenerlas como cadenas
          // });

          console.log(fechasRegistradas);

          // Abrir el modal y pasar las fechas registradas con el estado de pago
          this.dialog.open(CalendarioModalComponent, {
            data: {
              fechasRegistradas
            }, // Enviar las fechas con su estado de pago
            width: '500px',
          }).afterClosed().subscribe(resultado => {

            if (resultado === true) { // Solo actualizar si el modal indica que hubo cambios
              this.obtenerAsistencia(); // Método para obtener nuevamente las asistencias
            }

          });
        } else {
          alert('No se encontraron asistencias para este usuario.');
        }
      },
      (error) => console.error('Error al consultar asistencias:', error)
    );
  }

  nuevaAsistencia() {

    this.dialog.open(ModalAsistenciaComponent, {
      disableClose: true

    }).afterClosed().subscribe(resultado => {

      if (resultado === "true") this.obtenerAsistencia();

    });

  }


}
