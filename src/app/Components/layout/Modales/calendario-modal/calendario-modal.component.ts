import { ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MAT_DATE_FORMATS } from '@angular/material/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import moment, { Moment } from 'moment';
import Swal from 'sweetalert2';
import { Pagos } from '../../../../Interfaces/pagos';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { PagosService } from '../../../../Services/pagos.service';
import { AsignacionMembresia } from '../../../../Interfaces/asignacionMembresia';
import { VerImagenProductoModalComponent } from '../ver-imagen-producto-modal/ver-imagen-producto-modal.component';

export const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@Component({
  selector: 'app-calendario-modal',
  templateUrl: './calendario-modal.component.html',
  styleUrls: ['./calendario-modal.component.css'],
  providers: [
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS },
  ],
})
export class CalendarioModalComponent {
  fechasRegistradas: { fecha: moment.Moment, pagoRealizado: boolean, idUsuario: number, asistenciaId: number, imagenUrl: string }[] = [];
  selectedDate: Date | null = new Date();
  asignaciones: AsignacionMembresia[] | undefined;
  asignacionEncontrada?: AsignacionMembresia;
  mesActual = moment();
  diasSemana = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
  diasDelMes: number[] = [];
  diasPrevios: number[] = [];
  meses = [
    { nombre: 'Enero', valor: 0 },
    { nombre: 'Febrero', valor: 1 },
    { nombre: 'Marzo', valor: 2 },
    { nombre: 'Abril', valor: 3 },
    { nombre: 'Mayo', valor: 4 },
    { nombre: 'Junio', valor: 5 },
    { nombre: 'Julio', valor: 6 },
    { nombre: 'Agosto', valor: 7 },
    { nombre: 'Septiembre', valor: 8 },
    { nombre: 'Octubre', valor: 9 },
    { nombre: 'Noviembre', valor: 10 },
    { nombre: 'Diciembre', valor: 11 },
  ];
  anios: number[] = [];
  mesSeleccionado = this.mesActual.month();
  anioSeleccionado = this.mesActual.year();
  error: boolean | undefined;
  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  imagenClienteSeleccionada: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<CalendarioModalComponent>,
    private _usuarioServicio: UsuariosService,
    private _pagoServicio: PagosService,
     private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: { fechasRegistradas: { fecha: string, pagoRealizado: boolean, idUsuario: number, asistenciaId: number, imagenUrl: string }[] },
    private cdr: ChangeDetectorRef
  ) {

    this.generarCalendario();
    this.generarAnios();
    console.log('Fechas iniciales:', data);
    console.log('Fechas iniciales:', data.fechasRegistradas);
     this.imagenClienteSeleccionada = data.fechasRegistradas[0].imagenUrl;
    // Convierte las fechas y almacena el estado de pago
    this.fechasRegistradas = data.fechasRegistradas.map(fechaData => {
      const fechaMoment = moment(fechaData.fecha, 'DD/MM/YYYY');
      return { fecha: fechaMoment, pagoRealizado: fechaData.pagoRealizado, idUsuario: fechaData.idUsuario, asistenciaId: fechaData.asistenciaId, imagenUrl: fechaData.imagenUrl };
    });

    console.log('Fechas convertidas:', this.fechasRegistradas);
  }


  verImagen(): void {
    //  console.log(usuario);
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imagenes: [this.imagenClienteSeleccionada]
      }
    });
  }

  mesAnterior() {
    this.mesActual = this.mesActual.subtract(1, 'month');
    this.mesSeleccionado = this.mesActual.month();
    this.anioSeleccionado = this.mesActual.year();
    this.generarCalendario();
  }

  mesSiguiente() {
    this.mesActual = this.mesActual.add(1, 'month');
    this.mesSeleccionado = this.mesActual.month();
    this.anioSeleccionado = this.mesActual.year();
    this.generarCalendario();
  }

  cambiarMes() {
    this.mesActual = moment().year(this.anioSeleccionado).month(this.mesSeleccionado);
    this.generarCalendario();
  }

  cambiarAnio() {
    this.mesActual = moment().year(this.anioSeleccionado).month(this.mesSeleccionado);
    this.generarCalendario();
  }

  generarAnios() {
    const anioActual = moment().year();
    this.anios = Array.from({ length: 20 }, (_, i) => anioActual - 10 + i);
  }

  ngOnInit() {
    this.generarCalendario();
  }

  // generarCalendario() {
  //   const inicioMes = this.mesActual.clone().startOf('month');
  //   const diasEnMes = this.mesActual.daysInMonth();

  //   this.diasSemana = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

  //   const diasPrevios = inicioMes.day();
  //   this.diasPrevios = Array.from({ length: diasPrevios }, () => null);

  //   this.diasDelMes = Array.from({ length: diasEnMes }, (_, i) => i + 1);
  // }

  generarCalendario() {
    const inicioMes = moment().year(this.anioSeleccionado).month(this.mesSeleccionado).startOf('month');
    const diasEnMes = inicioMes.daysInMonth();
    const diaSemanaInicio = inicioMes.isoWeekday() % 7;

    this.diasPrevios = Array(diaSemanaInicio).fill(null);
    this.diasDelMes = Array.from({ length: diasEnMes }, (_, i) => i + 1);
  }



  esFechaRegistrada(dia: number): boolean {
    return this.fechasRegistradas.some(fechaData =>
      fechaData.fecha.date() === dia &&
      fechaData.fecha.month() === this.mesActual.month() &&
      fechaData.fecha.year() === this.mesActual.year()
    );
  }

  esPagoRealizado(dia: number): boolean {
    const fechaData = this.fechasRegistradas.find(fechaData =>
      fechaData.fecha.date() === dia &&
      fechaData.fecha.month() === this.mesActual.month() &&
      fechaData.fecha.year() === this.mesActual.year()
    );
    return fechaData ? fechaData.pagoRealizado : false;
  }


  // Método que maneja el clic en una fecha registrada
  onFechaClick(dia: number): void {
    const fechaData = this.fechasRegistradas.find(fechaData =>
      fechaData.fecha.date() === dia &&
      fechaData.fecha.month() === this.mesActual.month() &&
      fechaData.fecha.year() === this.mesActual.year()
    );

    if (fechaData) {
      // Si el pago no fue realizado
      if (fechaData.pagoRealizado == false) {
        Swal.fire({
          title: '¿Desea registrar el pago?',
          text: `¿Está seguro de que desea registrar el pago para el día ${dia} ?`,
          icon: 'question',
          showCancelButton: true,
          confirmButtonColor: '#1337E8',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Sí, registrar pago',
          cancelButtonText: 'Cancelar',
        }).then((result) => {
          if (result.isConfirmed) {
            // Si el usuario confirma, llamar al método para registrar el pago
            // this.registrarPago(dia, idUsuario);


            // Swal.fire({
            //   icon: 'error',
            //   title: 'Pago pendiente',
            //   text: `Aquii ${fechaData.asistenciaId} aún no ha sido realizado.`,
            // });


            const _pago: Pagos = {

              pagoId: 0,
              idAsistencia: fechaData.asistenciaId,
              idUsuario: fechaData.idUsuario,
              montoTexto: "0",
              metodoPago: "Efectivo",
              tipoPago: "string",
              fechaPago: "string",

              observaciones: "Cancelo un pago pendiente",


              // imageData: this.imageData,
            }


            this._pagoServicio.obtenerEstadoCalendario(_pago.idUsuario)
              .subscribe(
                (response) => {
                  if (response.status) {
                    response.data;
                    console.log(response.data);
                    if (response.data == "Membresia por meses" || response.data == "Membresia Anual") {

                      Swal.fire({
                        icon: 'error',
                        title: 'Pago',
                        text: `El pago solo se puede realizar para membresias de pago diario.`,
                      });
                      this.error = false;
                      return;

                    } else {

                      this.error = true;
                    }

                  } else {
                    console.error('Error: No se pudo obtener el estado del calendario');
                  }
                },
                (error) => {
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
                            this.onFechaClick(dia);
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

                () => {
                  // Este bloque se ejecuta cuando la suscripción se completa
                  if (this.error === true) {

                    // Swal.fire({
                    //   icon: 'success',
                    //   title: 'Pago',
                    //   text: `El pago para la fecha ${fechaData.fecha.format('DD/MM/YYYY')} aún no ha sido realizado.`,
                    // });

                    this._pagoServicio.registrarPagoCalendario(_pago).subscribe({

                      next: (data) => {
                        if (data.status) {
                          Swal.fire({
                            icon: 'success',
                            title: 'Pago Registrado',
                            text: `El Pago fue registrado`,
                          }).then(() => {
                            this.dialogRef.close(true); // Cierra el modal después de registrar el pago.
                          });


                        } else {
                          if (data.msg == "El nombre del correo ya existe.") {
                            Swal.fire({
                              icon: 'error',
                              title: 'ERROR',
                              text: `El nombre del correo ya existe.`,
                            });


                          } else if (data.msg == "El nombre del usuario ya existe.") {
                            Swal.fire({
                              icon: 'error',
                              title: 'ERROR',
                              text: `El nombre del usuario ya existe.`,
                            });
                          }

                          else {
                            Swal.fire({
                              icon: 'error',
                              title: 'ERROR',
                              text: `No se pudo eliminar el usuario`,
                            });

                          }

                          // this._utilidadServicio.mostrarAlerta("No se pudo registrar usuario ", "Error");
                        }
                      },
                      error: (e) => {


                        // Swal.fire({
                        //   icon: 'error',
                        //   title: 'ERROR',
                        //   text: ` el cliente  editar`,
                        // });
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
                                  this.onFechaClick(dia);
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
                      }



                    })





                  }


                }

              );





          }


        });

      } else {
        // Si el pago  fue realizado
        // Swal.fire({
        //   icon: 'success',
        //   title: 'Pago',
        //   text: `El pago para la fecha ${fechaData.fecha.format('DD/MM/YYYY')} aún no ha sido realizado.`,
        // });
      }
    }
  }






}


