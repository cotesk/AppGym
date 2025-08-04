import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { VerImagenProductoModalComponent } from '../../Modales/ver-imagen-producto-modal/ver-imagen-producto-modal.component';
import { Chart, registerables } from 'Chart.js'
import { DashBoardService } from '../../../../Services/dash-board.service';
import { from } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { DomSanitizer } from '@angular/platform-browser';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { jsPDF } from 'jspdf';
import 'jspdf-autotable';
import * as XLSX from 'xlsx';
import moment from 'moment';
import Swal from 'sweetalert2';
import { EmpresaService } from '../../../../Services/empresa.service';
import { Empresa } from '../../../../Interfaces/empresa';
import { CajaService } from '../../../../Services/caja.service';
import { Caja } from '../../../../Interfaces/caja';
import { UsuariosService } from '../../../../Services/usuarios.service';
import * as CryptoJS from 'crypto-js';
import { CellHookData } from 'jspdf-autotable';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';



Chart.register(...registerables);


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  animations: [
    trigger('productoHighlight', [
      state('highlighted-1', style({
        transform: 'scale(1.1)',
        boxShadow: '0 0 15px rgba(0, 0, 0, 0.3)',
        border: '12px solid #FFD700',
      })),
      state('highlighted-2', style({
        transform: 'scale(1.1)',
        boxShadow: '0 0 15px rgba(0, 0, 0, 0.3)',
        border: '12px solid #C0C0C0',
      })),
      state('highlighted-3', style({
        transform: 'scale(1.1)',
        boxShadow: '0 0 15px rgba(0, 0, 0, 0.3)',
        border: '12px solid #cd7f32',
      })),
      transition('* => highlighted-1', animate('300ms ease-out')),
      transition('* => highlighted-2', animate('300ms ease-out')),
      transition('* => highlighted-3', animate('300ms ease-out')),
    ]),
  ],
})

export class DashBoardComponent implements OnInit {
  @ViewChild('tablaProductos', { static: true }) tablaProductos!: ElementRef;

  totalIngresos: string = "0";
  totalEntrenadores: string = "0";
  totalUsuarios: string = "0";
  totalCliente: string = "0";
  clientesConMasAsistencias: any[] = []; //me contiene el top 3
  totalClientesConMasAsistencias: any[] = []; //me contiene todas las asistencia
  filtro = '';
  // Columnas de la tabla de clientes
  clientesColumns: string[] = ['imagen', 'nombre', 'asistencias'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  dataSource = new MatTableDataSource(this.totalClientesConMasAsistencias);
  // Agrega una propiedad para controlar la visibilidad de la tabla
  mostrarTabla: boolean = false;

  TotalCaja: string = "0";
  NombreCaja: string = "No Registrado";
  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  totalEgresos: string = "0";
  myChart: Chart | undefined;


  isMobile: boolean = false;
  constructor(
    private _dashboardServicio: DashBoardService,
    public dialog: MatDialog,
    private sanitizer: DomSanitizer,
    private elRef: ElementRef,
    private empresaService: EmpresaService,
    private cajaService: CajaService,
    private _usuarioServicio: UsuariosService,
  ) {


  }
  @HostListener('window:resize', ['$event'])
  onResize(event: Event) { // Especificamos el tipo de evento
    this.checkMobile();
  }


  ngOnInit(): void {

    this.checkMobile();

    this._dashboardServicio.resumen().subscribe({
      next: (data) => {

        if (data.status) {

          this.totalIngresos = data.value.totalIngresos;
          this.totalCliente = data.value.totalCliente;
          this.totalUsuarios = data.value.totalUsuarios;
          this.totalCliente = data.value.totalCliente;
          this.totalEntrenadores = data.value.totalEntrenadores;

          // Cargar los 3 clientes con más asistencias
          this.clientesConMasAsistencias = data.value.clientesConMasAsistencias;
          // Cargar todos
          this.totalClientesConMasAsistencias = data.value.totalClientesConMasAsistencias;

          this.actualizarDataSource();

          const arrayData: any[] = data.value.pagoUltimaSemana;
          const arrayData2: any[] = data.value.pagosDoceMeses;

          //
          // const arrayData: any[] = data.value.ventasUltimaSemana.filter((value: { anulada: boolean }) => !value.anulada);

          const labelTemp = arrayData.map((value) => value.fecha);
          const dataTemp = arrayData.map((value) => value.total);
          this.mostrarGrafico(labelTemp, dataTemp)


          const labelTemp2 = arrayData2.map((value) => value.fecha);
          const dataTemp2 = arrayData2.map((value) => value.total);
          this.mostrarGraficoDoceMeses(labelTemp2, dataTemp2)




        }
      },
      error: (e) => {

        let idUsuario: number = 0;


        // Obtener el idUsuario del localStorage
        const usuarioString = localStorage.getItem('usuario');
        const bytes = CryptoJS.AES.decrypt(usuarioString!, this.CLAVE_SECRETA);
        const datosDesencriptados = bytes.toString(CryptoJS.enc.Utf8);
        if (datosDesencriptados !== null) {
          const usuario = JSON.parse(datosDesencriptados);
          idUsuario = usuario.idUsuario; // Obtener el idUsuario del objeto usuario

          this._usuarioServicio.obtenerUsuarioPorId(idUsuario).subscribe(
            (usuario: any) => {

              console.log('Usuario obtenido:', usuario);
              let refreshToken = usuario.refreshToken

              // Manejar la renovación del token
              this._usuarioServicio.renovarToken(refreshToken).subscribe(
                (response: any) => {
                  console.log('Token actualizado:', response.token);
                  // Guardar el nuevo token de acceso en el almacenamiento local
                  localStorage.setItem('authToken', response.token);
                  this.GraficaVenta();
                },
                (error: any) => {
                  console.error('Error al actualizar el token:', error);
                }
              );



            },
            (error: any) => {
              console.error('Error al obtener el usuario:', error);
            }
          );
        }


      },
      complete: () => { }
    })



    this.Caja();
  }


  // Método para actualizar el dataSource con el filtro
  actualizarDataSource() {
    // Filtrar los datos según el valor del campo de búsqueda
    if (this.filtro) {
      const filteredData = this.totalClientesConMasAsistencias.filter(cliente =>
        cliente.nombre.toLowerCase().includes(this.filtro.toLowerCase())
      );
      this.dataSource = new MatTableDataSource(filteredData);
    } else {
      this.dataSource = new MatTableDataSource(this.totalClientesConMasAsistencias);
    }

    // Asignar el paginator
    this.dataSource.paginator = this.paginator;
  }

  // Método que se ejecuta cuando el valor del filtro cambia
  onFiltroChange() {
    this.actualizarDataSource();
  }

  checkMobile() {
    this.isMobile = window.innerWidth <= 768; // Ajusta el ancho según tus necesidades
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
  }

  // ngAfterViewInit() {
  //   if (this.topProductosMasVendidos.length > 0) {
  //     this.renderizarGraficoDoughnut();
  //   }
  // }
  // renderizarGraficoDoughnut() {
  //   console.log('Entrando en renderizarGraficoDoughnut');
  //   let  nombresProductos ;
  //   if(this.isMobile== true){
  //      nombresProductos = this.topProductosMasVendidos.slice(0, 3).map(item => {
  //       return item.nombre.length > 12 ? item.nombre.slice(0, 12) + '...' : item.nombre;
  //     });
  //   }else{
  //      nombresProductos = this.topProductosMasVendidos.slice(0, 3).map(item => {
  //       return item.nombre.length > 20 ? item.nombre.slice(0, 20) + '...' : item.nombre;
  //     });
  //   }


  //   const cantidadesVendidas = this.topProductosMasVendidos.slice(0, 3).map(item => item.cantidadVendida);

  //   console.log('nombresProductos:', nombresProductos);
  //   console.log('cantidadesVendidas:', cantidadesVendidas);

  //   const canvas = document.getElementById('doughnutChart') as HTMLCanvasElement;
  //   console.log('Canvas:', canvas);

  //   const ctx = canvas.getContext('2d');
  //   console.log('Contexto 2D:', ctx);

  //   // Limpiar y renderizar nombres de productos
  //   const productNamesDiv = document.getElementById('productNames');
  //   if (productNamesDiv) {
  //     productNamesDiv.innerHTML = '';
  //     const colors = ['color-oro', 'color-plata', 'color-bronce'];

  //     // Iterar sobre los nombres de productos y crear elementos
  //     nombresProductos.forEach((nombre, index) => {
  //       const productNameElement = document.createElement('div');
  //       productNameElement.classList.add('product-name');

  //       // Crear el cuadro de color y establecer la clase correspondiente
  //       const colorBox = document.createElement('div');
  //       colorBox.classList.add('color-box');
  //       colorBox.classList.add(colors[index]); // Agregar clase dinámica

  //       // Añadir el cuadro de color y el texto al elemento del nombre del producto
  //       productNameElement.appendChild(colorBox);
  //       productNameElement.appendChild(document.createTextNode(`${index + 1}° Puesto: ${nombre}`));

  //       // Agregar el elemento del nombre del producto al contenedor
  //       productNamesDiv.appendChild(productNameElement);
  //     });
  //   }

  //   // Verificar si se pudo obtener el contexto 2D del canvas
  //   if (ctx) {
  //     // Configurar el gráfico usando la librería Chart.js
  //     new Chart(ctx, {
  //       type: 'doughnut',
  //       data: {
  //         labels: ["1° Puesto", "2° Puesto", "3° Puesto"],
  //         datasets: [{
  //           label: 'Cantidad Vendida',
  //           data: cantidadesVendidas,
  //           backgroundColor: [
  //             'rgba(255, 215, 0, 0.5)', // Oro
  //             'rgba(192, 192, 192, 0.5)', // Plata
  //             'rgba(205, 127, 50, 0.5)', // Bronce
  //           ],
  //           borderColor: [
  //             'rgba(255, 215, 0, 1)', // Oro (borde sólido)
  //             'rgba(192, 192, 192, 1)', // Plata (borde sólido)
  //             'rgba(205, 127, 50, 1)', // Bronce (borde sólido)
  //           ],
  //           borderWidth: 1,
  //         }],
  //       },
  //       options: {
  //         responsive: false,
  //         plugins: {
  //           legend: {
  //             position: 'top',
  //           },
  //           tooltip: {
  //             callbacks: {
  //               label: function (tooltipItem: any) {
  //                 return tooltipItem.label + ': ' + tooltipItem.raw.toFixed(0);
  //               },
  //             },
  //           },
  //         },
  //       },
  //     });
  //   } else {
  //     console.log('Error: No se pudo obtener el contexto 2D del Canvas.');
  //   }
  // }



  GraficaVenta() {
    this._dashboardServicio.resumen().subscribe({
      next: (data) => {

        if (data.status) {


          this.totalIngresos = data.value.totalIngresos;
          this.totalCliente = data.value.totalCliente;
          this.totalUsuarios = data.value.totalUsuarios;
          this.totalCliente = data.value.totalCliente;
          this.totalEntrenadores = data.value.totalEntrenadores;

          const arrayData: any[] = data.value.pagoUltimaSemana;
          const arrayData2: any[] = data.value.pagosDoceMeses;
          // const arrayData: any[] = data.value.ventasUltimaSemana.filter((value: { anulada: boolean }) => !value.anulada);

          const labelTemp = arrayData.map((value) => value.fecha);
          const dataTemp = arrayData.map((value) => value.total);
          this.mostrarGrafico(labelTemp, dataTemp)


          const labelTemp2 = arrayData2.map((value) => value.fecha);
          const dataTemp2 = arrayData2.map((value) => value.total);
          this.mostrarGraficoDoceMeses(labelTemp2, dataTemp2)

        }
      },
      error: (e) => {

        let idUsuario: number = 0;


        // Obtener el idUsuario del localStorage
        const usuarioString = localStorage.getItem('usuario');
        const bytes = CryptoJS.AES.decrypt(usuarioString!, this.CLAVE_SECRETA);
        const datosDesencriptados = bytes.toString(CryptoJS.enc.Utf8);
        if (datosDesencriptados !== null) {
          const usuario = JSON.parse(datosDesencriptados);
          idUsuario = usuario.idUsuario; // Obtener el idUsuario del objeto usuario

          this._usuarioServicio.obtenerUsuarioPorId(idUsuario).subscribe(
            (usuario: any) => {

              console.log('Usuario obtenido:', usuario);
              let refreshToken = usuario.refreshToken

              // Manejar la renovación del token
              this._usuarioServicio.renovarToken(refreshToken).subscribe(
                (response: any) => {
                  console.log('Token actualizado:', response.token);
                  // Guardar el nuevo token de acceso en el almacenamiento local
                  localStorage.setItem('authToken', response.token);
                  this.GraficaVenta();
                },
                (error: any) => {
                  console.error('Error al actualizar el token:', error);
                }
              );



            },
            (error: any) => {
              console.error('Error al obtener el usuario:', error);
            }
          );
        }


      },
      complete: () => { }
    })
  }


  mostrarGraficoDoceMeses(labelsGrafico: any[], dataGrafico: any[]) {


    // const backgroundColors = labelsGrafico.map(() => dynamicColors());
    const dynamicColors = (value: number) => {

      const r = Math.floor(Math.random() * 150) + 100; // Componente rojo en el rango 100-250
      const g = Math.floor(Math.random() * 150) + 100; // Componente verde en el rango 100-250
      const b = Math.floor(Math.random() * 150) + 100; // Componente azul en el rango 100-250
      return `rgba(${r}, ${g}, ${b}, 0.7)`;

    };


    const backgroundColors = dataGrafico.map(value => dynamicColors(value));

    const monthNames = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    // Convertir las fechas en labelsGrafico a nombres de meses
    const labelsFormatted = labelsGrafico.map((dateString) => {
      const parts = dateString.split('/'); // Separar el mes y el año
      const monthIndex = parseInt(parts[0]) - 1; // Obtener el índice del mes (restar 1 porque los meses en JavaScript son base 0)
      const year = parts[1]; // Obtener el año

      return `${monthNames[monthIndex]} ${year}`; // Construir el nombre del mes y año
    });

    const myChart = new Chart('myChartDoce', {
      type: 'bar',
      data: {
        labels: labelsFormatted,
        datasets: [{
          label: '# de Pagos',
          data: dataGrafico,
          backgroundColor: backgroundColors,
          borderWidth: 0,
          borderRadius: 5
        },

        {
          type: 'line',
          label: 'Tramo de Pagos',
          data: dataGrafico,
          borderColor: 'black',
        }

        ]
      },

      options: {
        maintainAspectRatio: false,
        responsive: false,
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }

    });



  }

  mostrarGrafico(labelsGrafico: any[], dataGrafico: any[]) {



    // const backgroundColors = labelsGrafico.map(() => dynamicColors());
    const dynamicColors = (value: number) => {

      const r = Math.floor(Math.random() * 150) + 100; // Componente rojo en el rango 100-250
      const g = Math.floor(Math.random() * 150) + 100; // Componente verde en el rango 100-250
      const b = Math.floor(Math.random() * 150) + 100; // Componente azul en el rango 100-250
      return `rgba(${r}, ${g}, ${b}, 0.7)`;

    };


    const backgroundColors = dataGrafico.map(value => dynamicColors(value));

    // const cantidadObjetiva = localStorage.getItem('ventaObjetiva') || '20'; // Valor por defecto si no hay nada en el local storage
    // const nuevaLinea = Array(dataGrafico.length).fill(parseFloat(cantidadObjetiva));

    const myChart = new Chart('myChart', {
      type: 'bar',
      data: {
        labels: labelsGrafico,
        datasets: [{
          label: '# de Pagos',
          data: dataGrafico,
          backgroundColor: backgroundColors,
          borderWidth: 0,
          borderRadius: 5
        },
        {
          type: 'line',
          label: 'Tramo de Pagos',
          data: dataGrafico,
          borderColor: 'black',
        }]
      },

      options: {
        maintainAspectRatio: false,
        responsive: false,
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }

    });



    const myChartCircular = new Chart('myChartCircular', {
      type: 'doughnut',
      data: {
        labels: labelsGrafico,
        datasets: [{
          label: '# de Pagos',
          data: dataGrafico,
          backgroundColor: backgroundColors,
          borderWidth: 0,
          borderRadius: 5
        }]
      },

      options: {
        maintainAspectRatio: false,
        responsive: false,
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    });

  }

  Caja() {


    // Inicializar las variables
    let idUsuario: number = 0;
    let idCaja: number = 0;
    let transaccionesTexto: string | undefined;
    let Prestamos: string | undefined;
    let Devoluciones: string | undefined;
    let Gastos: string | undefined;
    let Ingreso: string | undefined;
    let Inicial: string | undefined;
    let nombreUsuario: string | undefined;
    // Obtener el idUsuario del localStorage
    const usuarioString = localStorage.getItem('usuario');
    const bytes = CryptoJS.AES.decrypt(usuarioString!, this.CLAVE_SECRETA);
    const datosDesencriptados = bytes.toString(CryptoJS.enc.Utf8);
    if (datosDesencriptados !== null) {
      const usuario = JSON.parse(datosDesencriptados);
      idUsuario = usuario.idUsuario; // Obtener el idUsuario del objeto usuario
    }

    if (idUsuario !== 0) {
      this.cajaService.obtenerCajaPorUsuario(idUsuario).subscribe({
        next: (caja: Caja | null) => {
          if (caja !== null) {
            // Si se encuentra una caja abierta para el idUsuario
            idCaja = caja.idCaja;
            transaccionesTexto = caja.transacciones;
            Prestamos = caja.prestamos;
            Devoluciones = caja.devoluciones;
            Gastos = caja.gastos;
            Ingreso = caja.ingresos;
            Inicial = caja.saldoInicial;
            nombreUsuario = caja.nombreUsuario;
            // Convertir las variables de string a number y realizar la suma
            const sumaTotal: number = (Ingreso !== undefined && Inicial !== undefined)
              ? parseFloat(Ingreso) + parseFloat(Inicial) : NaN;

            const RestaTotal: number = (Gastos !== undefined && Prestamos !== undefined)
              ? parseFloat(Gastos) + parseFloat(Prestamos) : NaN;

            const Resultado = sumaTotal - RestaTotal;

            this.TotalCaja = Resultado.toString();
            this.NombreCaja = nombreUsuario.toString();

            const cajaActualizada: Caja = {
              idCaja: idCaja,
              transaccionesTexto: transaccionesTexto,
              ingresosTexto: Ingreso,
              gastosTexto: Gastos,
              devolucionesTexto: Devoluciones,
              prestamosTexto: Prestamos,
              saldoInicialTexto: Inicial,
              estado: '',
              nombreUsuario: '',
              idUsuario: idUsuario
            };
            //  this.actualizarCaja(cajaActualizada);




          } else {
            // Manejar el caso en el que no se encuentre una caja abierta para el idUsuario
            // Swal.fire({
            //   icon: 'error',
            //   title: 'Error',
            //   text: 'No se encontró una caja abierta para el usuario actual',
            //   confirmButtonText: 'Aceptar'
            // });
            // // Detener la ejecución
            // return;
          }
        },
        error: (error) => {
          // console.error('Error al obtener la caja abierta:', error);
          // Swal.fire({
          //   icon: 'error',
          //   title: 'Error',
          //   text: 'Este usuario no tiene una caja definida, define una caja para poder realizar una venta ',
          //   confirmButtonText: 'Aceptar'
          // });
          // // Detener la ejecución
          // return;
          let idUsuario: number = 0;


          // Obtener el idUsuario del localStorage
          const usuarioString = localStorage.getItem('usuario');
          const bytes = CryptoJS.AES.decrypt(usuarioString!, this.CLAVE_SECRETA);
          const datosDesencriptados = bytes.toString(CryptoJS.enc.Utf8);
          if (datosDesencriptados !== null) {
            const usuario = JSON.parse(datosDesencriptados);
            idUsuario = usuario.idUsuario; // Obtener el idUsuario del objeto usuario

            this._usuarioServicio.obtenerUsuarioPorId(idUsuario).subscribe(
              (usuario: any) => {

                console.log('Usuario obtenido:', usuario);
                let refreshToken = usuario.refreshToken

                // Manejar la renovación del token
                this._usuarioServicio.renovarToken(refreshToken).subscribe(
                  (response: any) => {
                    console.log('Token actualizado:', response.token);
                    // Guardar el nuevo token de acceso en el almacenamiento local
                    localStorage.setItem('authToken', response.token);
                    this.Caja();
                  },
                  (error: any) => {
                    console.error('Error al actualizar el token:', error);
                  }
                );



              },
              (error: any) => {
                console.error('Error al obtener el usuario:', error);
              }
            );
          }



        },



      });
    } else {
      console.log('No se encontró el idUsuario en el localStorage');
    }

  }
  mostrarTodosProductos(): void {
    this.mostrarTabla = !this.mostrarTabla;
  }

  abrirDialogImagen(imagenData: string): void {
    console.log(imagenData); // Verifica que la URL sea correcta
    const dialogRef = this.dialog.open(VerImagenProductoModalComponent, {
      data: { imageData: imagenData }
    });
  }
  onMouseEnter(producto: any, index: number): void {
    producto.estadoAnimacion = `highlighted-${index}`;
  }

  onMouseLeave(producto: any): void {
    producto.estadoAnimacion = 'normal'; // Agrega una propiedad estadoAnimacion en tu objeto producto
  }



  // private actualizarTopProductosMasVendidos() {
  //   // Ordenar la lista de productos por cantidad vendida de forma descendente
  //   this.topProductosMasVendidos = this.productosMasVendidos.sort((a, b) => b.cantidadVendida - a.cantidadVendida).slice(0,3);
  // }

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
  configurarVentaObjetiva(): void {

    Swal.fire({
      title: '¿Defina su metas de ventas?',
      input: 'radio',
      inputOptions: {
        diario: 'Diario',
        mensual: 'Mensual'
      },
      inputValidator: (value) => {
        if (!value) {
          return 'Por favor selecciona una opción';
        }
        return undefined; // Devuelve undefined cuando no hay errores
      },
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Confirmar',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        if (result.value === 'mensual') {


          Swal.fire({
            title: 'Configurar Meta De Venta',
            input: 'number',
            inputLabel: 'Ingrese la cantidad para meta de venta mensual',
            inputAttributes: {
              min: '0',
              step: '1'
            },
            showCancelButton: true,
            confirmButtonColor: '#1337E8',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Guardar',
            cancelButtonText: 'Cancelar',
            showLoaderOnConfirm: true,
            preConfirm: (cantidad) => {
              return new Promise<void>((resolve) => {
                if (isNaN(cantidad) || cantidad < 0) {
                  Swal.showValidationMessage('Por favor ingrese una cantidad válida.');
                } else {
                  localStorage.setItem('ventaObjetivaMensual', cantidad);
                  resolve();
                }
              });
            }
          }).then((result) => {
            if (result.isConfirmed) {
              Swal.fire({
                icon: 'success',
                title: '¡Meta de venta mensual configurada!',
                text: `La cantidad de venta mensual es de: ${result.value}`
              });
              setTimeout(() => {
                location.reload();
              }, 1000); // Cambia el valor del tiempo de espera según tus necesidades
              // const cantidadObjetiva = localStorage.getItem('ventaObjetiva') || '20';
              // const nuevaLinea = Array(31).fill(parseFloat(cantidadObjetiva));

              // // Verificar si el gráfico está definido antes de actualizarlo
              // if (this.myChart !== undefined) {
              //   this.myChart.data.datasets[1].data = nuevaLinea;
              //   this.myChart.update(); // Actualizar el gráfico
              // } else {
              //   console.error('myChart no está definido');
              // }
            }
          });


        } else if (result.value === 'diario') {


          Swal.fire({
            title: 'Configurar Meta De Venta',
            input: 'number',
            inputLabel: 'Ingrese la cantidad para meta de venta diaria',
            inputAttributes: {
              min: '0',
              step: '1'
            },
            showCancelButton: true,
            confirmButtonColor: '#1337E8',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Guardar',
            cancelButtonText: 'Cancelar',
            showLoaderOnConfirm: true,
            preConfirm: (cantidad) => {
              return new Promise<void>((resolve) => {
                if (isNaN(cantidad) || cantidad < 0) {
                  Swal.showValidationMessage('Por favor ingrese una cantidad válida.');
                } else {
                  localStorage.setItem('ventaObjetiva', cantidad);
                  resolve();
                }
              });
            }
          }).then((result) => {
            if (result.isConfirmed) {
              Swal.fire({
                icon: 'success',
                title: '¡Meta de venta diaria configurada!',
                text: `La cantidad de venta diaria es: ${result.value}`
              });
              setTimeout(() => {
                location.reload();
              }, 1000); // Cambia el valor del tiempo de espera según tus necesidades

            }
          });



        }
      }
    });






  }








}
