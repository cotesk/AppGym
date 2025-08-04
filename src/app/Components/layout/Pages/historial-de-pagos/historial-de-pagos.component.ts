import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { Pagos } from '../../../../Interfaces/pagos';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { PagosService } from '../../../../Services/pagos.service';
import Swal from 'sweetalert2';
import { HistorialPago } from '../../../../Interfaces/historialPago';

@Component({
  selector: 'app-historial-de-pagos',
  templateUrl: './historial-de-pagos.component.html',
  styleUrl: './historial-de-pagos.component.css'
})
export class HistorialDePagosComponent implements OnInit, AfterViewInit {


  columnasTabla: string[] = ['historialPagoId','nombreUsuario', 'fechaPago','montoTexto' ,'tipoPago', 'acciones'];
  dataInicio: HistorialPago[] = [];
  dataListaPago = new MatTableDataSource(this.dataInicio);
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
    private _pagoServicio: PagosService,
    private _utilidadServicio: UtilidadService,
    private _usuarioServicio: UsuariosService,

  ) { }


   obtenerPagos() {

    this._pagoServicio.listaPaginada(this.page, this.pageSize, this.searchTerm).subscribe({
      next: (data) => {
        if (data && data.data && data.data.length > 0) {

          console.log('data :'+data.data)

          this.totalUsuario = data.total;
          this.totalPages = data.totalPages;
          // this.dataListaUsuarios.data = data.data;
          this.dataListaPago.data = data.data;
        } else {
          this.totalUsuario = 0; // Reinicia el total de categorías si no hay datos
          this.totalPages = 0; // Reinicia el total de páginas si no hay datos
          this.dataListaPago.data = []; // Limpia los datos existentes
          // Swal.fire({
          //   icon: 'warning',
          //   title: 'Advertencia',
          //   text: 'No se encontraron datos',
          // });
        }
      },
      error: (error) => this.handleTokenError(() => this.obtenerPagos())
    });

  }
  handleTokenError(retryCallback: () => void): void {
    this.totalUsuario = 0;
    this.totalPages = 0;
    this.dataListaPago.data = [];
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
    this.obtenerPagos();

  }


  ngAfterViewInit(): void {
    this.dataListaPago.paginator = this.paginacionTabla;
  }




  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.obtenerPagos();
  }

  aplicarFiltroTabla(event: Event) {
    const filtroValue = (event.target as HTMLInputElement).value.trim().toLowerCase();

      this.searchTerm = filtroValue;


    this.obtenerPagos();
  }
  firstPage() {
    this.page = 1;
    this.obtenerPagos();
  }

  previousPage() {
    if (this.page > 1) {
      this.page--;
      this.obtenerPagos();
    }
  }

  nextPage() {
    if (this.page < this.totalPages) {
      this.page++;
      this.obtenerPagos();
    }
  }

  lastPage() {
    this.page = this.totalPages;
    this.obtenerPagos();
  }
  pageSizeChange() {
    this.page = 1;
    this.obtenerPagos();
  }




  formatearNumero(numero: string): string {
    // Convierte la cadena a número
    const valorNumerico = parseFloat(numero.replace(',', '.'));

    // Verifica si es un número válido
    if (!isNaN(valorNumerico)) {
      // Formatea el número con comas como separadores de miles y dos dígitos decimales
      return valorNumerico.toLocaleString('es-CO', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    } else {
      // Devuelve la cadena original si no se puede convertir a número
      return numero;
    }
  }







}
