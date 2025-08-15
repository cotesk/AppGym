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
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { VerImagenProductoModalComponent } from '../../Modales/ver-imagen-producto-modal/ver-imagen-producto-modal.component';

@Component({
  selector: 'app-historial-de-pagos',
  templateUrl: './historial-de-pagos.component.html',
  styleUrl: './historial-de-pagos.component.css'
})
export class HistorialDePagosComponent implements OnInit, AfterViewInit {


  columnasTabla: string[] = ['historialPagoId', 'imagen','nombreUsuario', 'fechaPago', 'montoTexto', 'tipoPago'];
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

          console.log('data :' + data)

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


 verImagen(usuario: any): void {
    // console.log(usuario);
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imagenes: [usuario.imagenUrl]
      }
    });
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


  private parseMonto(valor: string | number): number {
    if (typeof valor === 'number') return valor;
    if (!valor) return 0;
    return Number(valor); // Convierte "60000.00" -> 60000
  }

  private formatCOP(n: number): string {
    return n.toLocaleString('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });
  }

  private parseFechaDDMMYYYY(fechaHora: string): { d: number; m: number; y: number } {
    // Espera "09/08/2025 01:16 PM" o "09/08/2025"
    const fecha = fechaHora?.split(' ')[0] ?? '';
    const [dd, mm, yyyy] = fecha.split('/').map(Number);
    return { d: dd, m: mm, y: yyyy };
  }

  /* ---------- Flujo principal ---------- */
  generarPdf() {
    Swal.fire({
      title: 'Seleccione tipo de reporte',
      input: 'select',
      inputOptions: { mes: 'Por mes', dia: 'Por día' },
      inputPlaceholder: 'Elige una opción',
      showCancelButton: true,
      confirmButtonText: 'Siguiente',
      cancelButtonText: 'Cancelar'
    }).then((res) => {
      if (!res.isConfirmed) return;
      const tipo = res.value;
      if (tipo === 'mes') this.pedirMesYAño();
      if (tipo === 'dia') this.pedirDiaMesAño();
    });
  }

  private pedirMesYAño() {
    Swal.fire({
      title: 'Mes y Año',
      html: `
      <input id="month" type="number" class="swal2-input" placeholder="Mes (1-12)" min="1" max="12">
      <input id="year" type="number" class="swal2-input" placeholder="Año" value="${new Date().getFullYear()}">
    `,
      showCancelButton: true,
      confirmButtonColor: '#1337E8',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Generar PDF',
      cancelButtonText: 'Cancelar',
      preConfirm: () => {
        const month = Number((document.getElementById('month') as HTMLInputElement).value);
        const year = Number((document.getElementById('year') as HTMLInputElement).value);
        if (!month || month < 1 || month > 12 || !year) {
          Swal.showValidationMessage('Ingresa un mes (1-12) y un año válido.');
        }
        return { month, year };
      }
    }).then(r => { if (r.isConfirmed) this.generarPDFPorMes(r.value.month, r.value.year); });
  }

  private pedirDiaMesAño() {
    Swal.fire({
      title: 'Día, Mes y Año',
      html: `
      <input id="day" type="number" class="swal2-input" placeholder="Día (1-31)" min="1" max="31">
      <input id="month" type="number" class="swal2-input" placeholder="Mes (1-12)" min="1" max="12">
      <input id="year" type="number" class="swal2-input" placeholder="Año" value="${new Date().getFullYear()}">
    `,
      showCancelButton: true,
      confirmButtonColor: '#1337E8',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Generar PDF',
      cancelButtonText: 'Cancelar',
      preConfirm: () => {
        const day = Number((document.getElementById('day') as HTMLInputElement).value);
        const month = Number((document.getElementById('month') as HTMLInputElement).value);
        const year = Number((document.getElementById('year') as HTMLInputElement).value);
        if (!day || day < 1 || day > 31 || !month || month < 1 || month > 12 || !year) {
          Swal.showValidationMessage('Ingresa día (1-31), mes (1-12) y año válidos.');
        }
        return { day, month, year };
      }
    }).then(r => { if (r.isConfirmed) this.generarPDFPorDia(r.value.day, r.value.month, r.value.year); });
  }

  /* ---------- Generación con totales ---------- */
  private generarPDFPorMes(mes: number, anio: number) {
    const filtrados = this.dataListaPago.data.filter(p => {
      const { m, y } = this.parseFechaDDMMYYYY(p.fechaPago);
      return m === mes && y === anio;
    });

    if (!filtrados.length) {
      Swal.fire('Sin datos', 'No hay pagos para ese mes y año.', 'warning');
      return;
    }

    const totalMes = filtrados.reduce((acc, p) => acc + this.parseMonto(p.montoTexto), 0);
    this.crearPDF(filtrados, `Historial de Pagos - ${mes}/${anio}`, `Total del mes: ${this.formatCOP(totalMes)}`);
  }

  private generarPDFPorDia(dia: number, mes: number, anio: number) {
    const filtrados = this.dataListaPago.data.filter(p => {
      const { d, m, y } = this.parseFechaDDMMYYYY(p.fechaPago);
      return d === dia && m === mes && y === anio;
    });

    if (!filtrados.length) {
      Swal.fire('Sin datos', 'No hay pagos para esa fecha.', 'warning');
      return;
    }

    const totalDia = filtrados.reduce((acc, p) => acc + this.parseMonto(p.montoTexto), 0);
    this.crearPDF(filtrados, `Historial de Pagos - ${dia}/${mes}/${anio}`, `Total del día: ${this.formatCOP(totalDia)}`);
  }

  /* ---------- Construcción del PDF ---------- */
  private crearPDF(data: any[], titulo: string, totalTexto: string) {
    const doc = new jsPDF();

    doc.setFontSize(16);
    doc.text(titulo, 14, 14);

    autoTable(doc, {
      startY: 20,
      head: [['Id Historial', 'Nombre del Cliente', 'Fecha de Pago', 'Total', 'Tipo Membresia']],
      body: data.map(p => [
        p.historialPagoId,
        p.nombreUsuario,
        p.fechaPago,
        this.formatCOP(this.parseMonto(p.montoTexto)),
        p.tipoPago
      ]),
      styles: { fontSize: 10, halign: 'left' },
      headStyles: { halign: 'center' },
      columnStyles: {
        0: { halign: 'center' },
        1: { halign: 'center' },
        2: { halign: 'center' },
        3: { halign: 'center' },
        4: { halign: 'center' }
      }
    });

    const finalY = (doc as any).lastAutoTable?.finalY ?? 20;
    doc.setFontSize(12);
    doc.text(totalTexto, 14, finalY + 10);

    // 🔹 Abrir en nueva pestaña en lugar de descargar
    const blob = doc.output('blob');
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
  }



}
