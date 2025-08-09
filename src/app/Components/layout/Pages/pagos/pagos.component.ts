import { Component } from '@angular/core';
import { Usuario } from '../../../../Interfaces/usuario';
import { UsuariosService } from '../../../../Services/usuarios.service';
import { UtilidadService } from '../../../../Reutilizable/utilidad.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EmpresaService } from '../../../../Services/empresa.service';
import { CajaService } from '../../../../Services/caja.service';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import moment from 'moment';
import * as CryptoJS from 'crypto-js';
import { ReponseApi } from '../../../../Interfaces/reponse-api';
import Swal from 'sweetalert2';
import { MembresiaService } from '../../../../Services/membresia.service';
import { Membresia } from '../../../../Interfaces/membresia';
import { NonNullAssert } from '@angular/compiler';
import { Pagos } from '../../../../Interfaces/pagos';
import { PagosService } from '../../../../Services/pagos.service';
import { VerImagenProductoModalComponent } from '../../Modales/ver-imagen-producto-modal/ver-imagen-producto-modal.component';

@Component({
  selector: 'app-pagos',
  templateUrl: './pagos.component.html',
  styleUrl: './pagos.component.css'
})
export class PagosComponent {

  listaClientesFiltrada: Usuario[] = [];
  listaClientes: Usuario[] = [];
  clienteSeleccionado!: any | null;
  idUsuarioSeleccionado!: any | null;
  nombreMembresiaSeleccionado!: any | null;
  diasMembresiaSeleccionado!: any | null;
  precioMembresiaSeleccionado!: any | null;
  precioCopiaMembresiaSeleccionado!: any | null;
  activoMembresiaSeleccionado!: any | null;
  estadoMembresiaSeleccionado!: any | null;
  imagenSeleccionada: string | null = null;
  formularioPago: FormGroup;
  tipodePagoPorDefecto: string = "Efectivo";
  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  hayCajaAbierta: boolean = false;

  constructor(
    private fb: FormBuilder,
    private _utilidadServicio: UtilidadService,
    private empresaService: EmpresaService,
    private cajaService: CajaService,
    private _membresiaServicio: MembresiaService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private _usuarioServicio: UsuariosService,
    private _pagoServicio: PagosService,
  ) {

    this.formularioPago = this.fb.group({
      usuario: ['', Validators.required],
      observaciones: ['',],

    });
    const storedImageData = localStorage.getItem('imagenSeleccionada');
    if (storedImageData) {
      this.imagenSeleccionada = `data:image/png;base64, ${storedImageData}`;
    }

    this._usuarioServicio.listaClientes().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaClientes = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaClientes = lista.filter(p => p.esActivo == 1)


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
                  this.usuario();
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

    this.formularioPago.get('usuario')?.valueChanges.subscribe(value => {
      this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
    });

  }



  ngOnInit(): void {



    this.usuario();


    this.formularioPago = this.fb.group({
      usuario: ['', Validators.required],
      observaciones: ['',],

    });

    this.formularioPago.get('usuario')?.valueChanges.subscribe(value => {
      this.listaClientesFiltrada = this.retornarUsuariosPorFiltro(value);
    });

    this.obtenerCajasAbiertas();

  }


  usuario() {

    this._usuarioServicio.listaClientes().subscribe({

      next: (data) => {
        if (data.status) {

          this.listaClientes = data.value.sort((a: Usuario, b: Usuario) => a.nombreCompleto!.localeCompare(b.nombreCompleto!));

          const lista = data.value as Usuario[];
          this.listaClientes = lista.filter(p => p.esActivo == 1)


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
                  this.usuario();
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

  obtenerCajasAbiertas() {
    this.cajaService.lista().subscribe({
      next: (data) => {
        if (data && Array.isArray(data.value) && data.value.length > 0) {
          // Verificar si al menos una caja está abierta
          const cajaAbierta = data.value.find((caja: any) => caja.estado === 'Abierto');
          if (cajaAbierta) {
            // Si se encuentra al menos una caja abierta
            // Verificar si la fecha de inicio de la caja abierta coincide con la fecha actual
            const fechaInicioCaja = moment(cajaAbierta.fechaApertura);
            const fechaHoy = moment();
            if (fechaInicioCaja.isSame(fechaHoy, 'day')) {
              // Si la fecha de inicio coincide con la fecha actual, se puede proceder con la venta
              this.hayCajaAbierta = true;

            } else {
              // Si la fecha de inicio no coincide con la fecha actual, mostrar un mensaje de error
              Swal.fire({
                icon: 'error',
                title: '¡ ERROR !',
                text: 'Primero debe cerrar la caja antes de iniciar un nuevo pago.'
              });
            }
          } else {
            // Si no se encuentra ninguna caja abierta
            Swal.fire({
              icon: 'warning',
              title: 'Atención',
              text: 'No hay cajas abiertas'
            });
            this.hayCajaAbierta = false;
          }
        } else {
          this.hayCajaAbierta = false;
          Swal.fire({
            icon: 'warning',
            title: 'Atención',
            text: 'No hay cajas abiertas'
          });
        }
      },
      error: (error) => {
        let idUsuario: number = 0;


        // Obtener el idUsuario del localStorage
        const usuarioString = localStorage.getItem('usuario');
        const bytes = CryptoJS.AES.decrypt(usuarioString!, this.CLAVE_SECRETA!);
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
                  this.obtenerCajasAbiertas();
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
    });
  }

  retornarUsuariosPorFiltro(busqueda: any): Usuario[] {
    const valorBuscado = typeof busqueda === "string" ? busqueda.toLocaleLowerCase() : busqueda.nombreCompleto.toLocaleLowerCase()
    return this.listaClientes.filter(item => item.nombreCompleto!.toLocaleLowerCase().includes(valorBuscado));

  }
  mostrarListaClientes(): void {
    this.listaClientesFiltrada = this.listaClientes;
  }
  formatearNumero(numero: any): string {
    // Convierte la cadena a número

    // Verifica si es un número válido
    if (!isNaN(numero)) {
      // Formatea el número con comas como separadores de miles y dos dígitos decimales
      return numero.toLocaleString('es-CO', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    } else {
      // Devuelve la cadena original si no se puede convertir a número
      return numero;
    }
  }
  limpiarCampos() {


    this.precioMembresiaSeleccionado = null;
    this.precioCopiaMembresiaSeleccionado = null;
    this.nombreMembresiaSeleccionado = null;
    this.activoMembresiaSeleccionado = null;
    this.estadoMembresiaSeleccionado = null;

  }
  productoParaVenta(event: any) {
    // this.productoSeleccionado = event.option.value;


    const selectedProduct: Usuario = event.option.value;
    this.clienteSeleccionado = selectedProduct.nombreCompleto;
    this.idUsuarioSeleccionado = selectedProduct.idUsuario;

    this.cargarImagenUsuario(selectedProduct.idUsuario!);

    // Actualizar el valor del campo de búsqueda por código con el código del producto seleccionado
    this.formularioPago.get('usuario')?.setValue(selectedProduct.nombreCompleto);

    console.log("Datos recibidos:",  this.idUsuarioSeleccionado);

    this._membresiaServicio.getMembresiaByUsuario(selectedProduct.idUsuario!).subscribe({

      next: (data) => {
        if (data.status) {

          // this.listaClientes = data.value

          console.log("Datos recibidos:", data);

          this.diasMembresiaSeleccionado = data.value[0].membresia.duracionDias
          this.nombreMembresiaSeleccionado = data.value[0].membresia.nombre;
          this.precioMembresiaSeleccionado = data.value[0].membresia.precio;
          this.activoMembresiaSeleccionado = data.value[0].membresia.esActivo;
          this.estadoMembresiaSeleccionado = data.value[0].estado;
          this.precioCopiaMembresiaSeleccionado = this.precioMembresiaSeleccionado;
          let precio: any = this.precioMembresiaSeleccionado
          this.precioMembresiaSeleccionado = this.formatearNumero(precio);
          console.log("Nombre recibidos:", data.value[0]);

          // Swal.fire({
          //   icon: 'error',
          //   title: '¡ ERROR !',
          //   text: `El nombre de la membresía es: ${data.value[0].membresia.nombre}`
          // });

          // Limpiar campos si es necesario, por ejemplo si se cambia de cliente
          if (this.nombreMembresiaSeleccionado == null) {
            this.limpiarCampos();
          }

        } else {

          if (data.msg == "No se encontró una asignación para el usuario especificado.") {

            Swal.fire({
              icon: 'error',
              title: '¡ ERROR !',
              text: `No se encontró una asignación para el usuario especificado.`
            });
            this.limpiarCampos();
          }



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
                  this.usuario();
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
  mostrarUsuario(usuario: Usuario): string {

    return usuario.nombreCompleto!;

  }
  lastItem(item: any, list: any[]): boolean {
    return item === list[list.length - 1];
  }
  private cargarImagenUsuario(idProducto: number) {
    this._usuarioServicio.obtenerImagenUsuario(idProducto).subscribe(
      (response: any) => {
        if (response && response.imageData) {
          this.imagenSeleccionada = `data:image/png;base64,${response.imageData}`;
        } else {
          console.error('Imagen no disponible');
          this.imagenSeleccionada = 'ruta/de/imagen/predeterminada.png'; // O deja nulo
        }
      },
      (error: any) => {
        console.error('Error al cargar la imagen:', error);
        this.imagenSeleccionada = 'ruta/de/imagen/predeterminada.png'; // Imagen predeterminada si falla
      }
    );
  }

  verImagen(usuario: any): void {
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imageData: usuario
      }
    });
  }


  registrar() {

    if (this.nombreMembresiaSeleccionado == null) {

      Swal.fire({
        icon: 'error',
        title: '¡ ERROR !',
        text: `El usuario ${this.clienteSeleccionado} no tiene una membresia asignada.`
      });
      return
    } else {

      if (this.activoMembresiaSeleccionado == false) {

        Swal.fire({
          icon: 'error',
          title: '¡ ERROR !',
          text: `Esta membresia esta desactivada necesita ser activada para realizar el pago.`
        });
        return
      } else {

        if (this.estadoMembresiaSeleccionado == "Activado") {

          Swal.fire({
            icon: 'error',
            title: '¡ ERROR !',
            text: `No puedes registrar un pago con una membresia activa.`
          });
          return
        } else {



          let Tipo: string;
          if (this.diasMembresiaSeleccionado < 2) {

            Tipo = "diario";
          } else if (this.diasMembresiaSeleccionado >= 28 && this.diasMembresiaSeleccionado < 33) {
            Tipo = "mensual";
          } else {
            Tipo = "anual";
          }

          let obser = this.formularioPago.value.observaciones;

          if (obser == "" || obser == null) {
            obser = "Sin ninguna novedad"
          }

          const _pago: Pagos = {

            pagoId: 0,
            idAsistencia: 0,
            idUsuario: this.idUsuarioSeleccionado,
            montoTexto: (this.precioCopiaMembresiaSeleccionado).toString(),
            metodoPago: this.tipodePagoPorDefecto,
            tipoPago: Tipo,
            fechaPago: "string",

            observaciones: obser,


            // imageData: this.imageData,
          }


          this._pagoServicio.registrarPago(_pago).subscribe({

            next: (data) => {
              if (data.status) {
                Swal.fire({
                  icon: 'success',
                  title: 'Pago Registrado',
                  text: `El Pago fue registrado`,
                });

                // Limpiar formulario y datos seleccionados
                this.formularioPago.reset(); // Resetea los controles del formulario
                this.clienteSeleccionado = null;
                this.nombreMembresiaSeleccionado = null;
                this.activoMembresiaSeleccionado = null;
                this.estadoMembresiaSeleccionado = null;
                this.precioMembresiaSeleccionado = null;
                this.precioCopiaMembresiaSeleccionado = null;
                this.imagenSeleccionada = null;
                this.tipodePagoPorDefecto = "Efectivo";


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
                        this.registrar();
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



    }


  }

}
