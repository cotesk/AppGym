import { Component } from '@angular/core';
import { UsuariosService } from '../../Services/usuarios.service';
import { MembresiaService } from '../../Services/membresia.service';
import { UtilidadService } from '../../Reutilizable/utilidad.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import Swal from 'sweetalert2';
import * as CryptoJS from 'crypto-js';
import { EntrenadorClienteService } from '../../Services/entrenador.service';
import { VerImagenProductoModalComponent } from '../layout/Modales/ver-imagen-producto-modal/ver-imagen-producto-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { switchMap, map, catchError } from 'rxjs/operators';
import { EMPTY, of } from 'rxjs';


@Component({
  selector: 'app-consular-membresia',
  templateUrl: './consular-membresia.component.html',
  styleUrl: './consular-membresia.component.css'
})
export class ConsularMembresiaComponent {



  private readonly CLAVE_SECRETA = '9P#5a^6s@Lb!DfG2@17#Co-Tes#07';
  formularioUsuario: FormGroup;
  clienteSeleccionado!: any | null;
  nombreMembresiaSeleccionado!: any | null;
  diasMembresiaSeleccionado!: any | null;
  diasRestantesMembresia !: any | null;
  precioMembresiaSeleccionado!: any | null;
  precioCopiaMembresiaSeleccionado!: any | null;
  activoMembresiaSeleccionado!: any | null;
  estadoMembresiaSeleccionado!: any | null;
  fechaVencimientoSeleccionado!: any | null;
  NombreEntrenadorSeleccionado!: any | null;
  imagenSeleccionada: string | null = null;
  imagenSeleccionadaCliente: string | null = null;
  mostrarImagenEntrenador: boolean = true;

  constructor(
    private _entrenadorServicio: EntrenadorClienteService,
    private _mebresiaServicio: MembresiaService,
    private _utilidadServicio: UtilidadService,
    private _usuarioServicio: UsuariosService,
    private dialog: MatDialog,
    private fb: FormBuilder,

  ) {

    this.formularioUsuario = this.fb.group({

      tipoBusqueda: ['cedula', Validators.required],
      telefono: ['', [Validators.pattern('[0-9]*'), Validators.maxLength(10)]],
      cedula: ['', [Validators.pattern('[0-9]*'), Validators.maxLength(10)]],
      correo: ['', Validators.email]
    });


    // Ajustar validaciones dinámicamente según tipo de búsqueda
    this.formularioUsuario.get('tipoBusqueda')?.valueChanges.subscribe(tipo => {
      this.formularioUsuario.get('cedula')?.clearValidators();
      this.formularioUsuario.get('telefono')?.clearValidators();
      this.formularioUsuario.get('correo')?.clearValidators();

      if (tipo === 'cedula') {
        this.formularioUsuario.get('cedula')?.setValidators([Validators.required, Validators.pattern(/^[0-9]*$/), Validators.maxLength(12)]);
      } else if (tipo === 'telefono') {
        this.formularioUsuario.get('telefono')?.setValidators([Validators.required, Validators.pattern(/^[0-9]*$/), Validators.maxLength(10)]);
      } else if (tipo === 'correo') {
        this.formularioUsuario.get('correo')?.setValidators([Validators.required, Validators.email]);
      }

      this.formularioUsuario.get('cedula')?.updateValueAndValidity();
      this.formularioUsuario.get('telefono')?.updateValueAndValidity();
      this.formularioUsuario.get('correo')?.updateValueAndValidity();
    });

  }

  ngOnInit(): void {


  }


  verImagen(usuario: any): void {
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imagenes: [usuario]
      }
    });
  }

  //  obtenerAsignaciones() {

  //   this._mebresiaServicio.listarAsignaciones().subscribe({

  //     next: (data) => {
  //       if (data.status) {

  //         // data.value.sort((a: AsignacionMembresia, b: AsignacionMembresia) => a.nombreUsuario!.localeCompare(b.nombreUsuario!));
  //         // this.dataListaMembresia.data = data.value;


  //       } else {

  //         Swal.fire({
  //           icon: 'warning',
  //           title: 'Advertencia',
  //           text: `no se encontraron datos`,
  //         });
  //         // this._utilidadServicio.mostrarAlerta("no se encontraron datos", "Oops!");

  //       }

  //     },
  //     error: (e) => {






  //     }

  //   })
  // }


  BuscarUsuario() {
    const tipo = this.formularioUsuario.get('tipoBusqueda')?.value;

    if (tipo === 'cedula') {
      const cedula = this.formularioUsuario.get('cedula')?.value;
      this.obtenerUsuarioPorCedula(cedula);
    }
    if (tipo === 'correo') {
      const cedula = this.formularioUsuario.get('correo')?.value;
      this.obtenerUsuarioPorCorreo(cedula);
    }
    if (tipo === 'telefono') {
      const telefono = this.formularioUsuario.get('telefono')?.value;
      this.obtenerUsuarioPorTelefono(telefono);
    }
    else {

    }
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
  formatearFecha(fecha: string): string {
    const fechaObjeto = new Date(fecha);
    const opciones: Intl.DateTimeFormatOptions = {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true,
    };
    return fechaObjeto.toLocaleString('es-CO', opciones);
  }
  obtenerUsuarioPorCedula(cedula: any) {
    // const cedula = this.formularioUsuario.value.cedula;

    this._usuarioServicio.obtenerUsuarioPorCedula(cedula).pipe(
      switchMap((usuario: any) => {
        this.limpiarCampos();
        this.clienteSeleccionado = usuario.nombreCompleto;

        console.log(usuario);

        return this._mebresiaServicio.getMembresiaByUsuario2(usuario.idUsuario).pipe(
          catchError(() => {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: 'Este usuario aún no tiene asignada una membresía.',
            });
            this.mostrarImagenEntrenador = false;
            this.imagenSeleccionadaCliente = null;
            this.limpiarCampos();
            return of(null); // evita lanzar error y cortar el flujo feo
          }),
          map((data: any) => ({ usuario, membresia: data }))
        );
      }),
      switchMap(({ usuario, membresia }) => {
        if (!membresia) {
          return EMPTY; // Detiene la ejecución si no hay membresía
        }

        console.log(membresia);
        if (membresia?.status) {
          // ✅ Ahora sí cargamos la imagen porque sabemos que tiene membresía
          this.cargarImagenUsuario(usuario.idUsuario);

          const detalles = membresia.value[0].membresia;
          const fechaVencimiento = new Date(membresia.value[0].fechaVencimiento);
          const hoy = new Date();

          // Calcular diferencia en días
          let diasRestantes = 0; // Valor por defecto
          if (membresia.value[0].estado !== 'Pendiente') {
            const diffTime = fechaVencimiento.getTime() - hoy.getTime();
            diasRestantes = Math.max(Math.ceil(diffTime / (1000 * 60 * 60 * 24)), 0);
          }

          this.diasMembresiaSeleccionado = detalles.duracionDias;
          this.nombreMembresiaSeleccionado = detalles.nombre;
          this.precioMembresiaSeleccionado = this.formatearNumero(detalles.precio);
          this.activoMembresiaSeleccionado = detalles.esActivo;
          this.fechaVencimientoSeleccionado = this.formatearFecha(membresia.value[0].fechaVencimiento);
          this.estadoMembresiaSeleccionado = membresia.value[0].estado;
          this.diasRestantesMembresia = diasRestantes; // 👈 Nuevo campo con días restantes
          this.mostrarImagenEntrenador = true;
        } else {
          Swal.fire({
            icon: 'info',
            title: 'Información',
            text: 'No se encontró una asignación para el usuario especificado.',
          });
          this.mostrarImagenEntrenador = false;
          this.limpiarCampos();
          return EMPTY;
        }

        return this._entrenadorServicio.getAsignacionesPorCliente(usuario.idUsuario);
      })
    ).subscribe({
      next: (asignaciones: any) => {
        if (asignaciones && asignaciones.length > 0) {
          const entrenador = asignaciones[0];
          this.NombreEntrenadorSeleccionado = entrenador.nombreEntrenador;
          this.mostrarImagenEntrenador = true;
          this.cargarImagenUsuarioEntrenador(entrenador.entrenadorId);
        } else {
          // Swal.fire({
          //   icon: 'info',
          //   title: 'Información',
          //   text: 'Este usuario no tiene asignado un entrenador.',
          // });
          this.mostrarImagenEntrenador = false;
        }
      },
      error: (error: any) => {
        console.error(error);
        if (error.status === 401) {
          // Manejo de error 401 si es necesario
        } else if (error === "Ocurrió un error en la solicitud. Por favor, inténtelo de nuevo más tarde.") {
          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: 'Este número de cédula no existe.',
          });
          this.limpiarCampos();
        } else {
          // Swal.fire({
          //   icon: 'info',
          //   title: 'Informativo',
          //   text: 'Este usuario no tiene asignado un entrenador.',
          // });
          this.mostrarImagenEntrenador = false;
        }
      },
      complete: () => {
        console.log('Operación completada.');
      },
    });
  }


  obtenerUsuarioPorTelefono(telefono: any) {
    // const cedula = this.formularioUsuario.value.cedula;

    this._usuarioServicio.obtenerUsuarioPorTelefono(telefono).pipe(
      switchMap((usuario: any) => {
        this.limpiarCampos();
        this.clienteSeleccionado = usuario.nombreCompleto;

        console.log(usuario);

        return this._mebresiaServicio.getMembresiaByUsuario2(usuario.idUsuario).pipe(
          catchError(() => {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: 'Este usuario aún no tiene asignada una membresía.',
            });
            this.mostrarImagenEntrenador = false;
            this.imagenSeleccionadaCliente = null;
            this.limpiarCampos();
            return of(null); // evita lanzar error y cortar el flujo feo
          }),
          map((data: any) => ({ usuario, membresia: data }))
        );
      }),
      switchMap(({ usuario, membresia }) => {
        if (!membresia) {
          return EMPTY; // Detiene la ejecución si no hay membresía
        }

        console.log(membresia);
        if (membresia?.status) {
          // ✅ Ahora sí cargamos la imagen porque sabemos que tiene membresía
          this.cargarImagenUsuario(usuario.idUsuario);

          const detalles = membresia.value[0].membresia;
          const fechaVencimiento = new Date(membresia.value[0].fechaVencimiento);
          const hoy = new Date();

          // Calcular diferencia en días
          let diasRestantes = 0; // Valor por defecto
          if (membresia.value[0].estado !== 'Pendiente') {
            const diffTime = fechaVencimiento.getTime() - hoy.getTime();
            diasRestantes = Math.max(Math.ceil(diffTime / (1000 * 60 * 60 * 24)), 0);
          }

          this.diasMembresiaSeleccionado = detalles.duracionDias;
          this.nombreMembresiaSeleccionado = detalles.nombre;
          this.precioMembresiaSeleccionado = this.formatearNumero(detalles.precio);
          this.activoMembresiaSeleccionado = detalles.esActivo;
          this.fechaVencimientoSeleccionado = this.formatearFecha(membresia.value[0].fechaVencimiento);
          this.estadoMembresiaSeleccionado = membresia.value[0].estado;
          this.diasRestantesMembresia = diasRestantes; // 👈 Nuevo campo con días restantes
          this.mostrarImagenEntrenador = true;
        } else {
          Swal.fire({
            icon: 'info',
            title: 'Información',
            text: 'No se encontró una asignación para el usuario especificado.',
          });
          this.mostrarImagenEntrenador = false;
          this.limpiarCampos();
          return EMPTY;
        }

        return this._entrenadorServicio.getAsignacionesPorCliente(usuario.idUsuario);
      })
    ).subscribe({
      next: (asignaciones: any) => {
        if (asignaciones && asignaciones.length > 0) {
          const entrenador = asignaciones[0];
          this.NombreEntrenadorSeleccionado = entrenador.nombreEntrenador;
          this.mostrarImagenEntrenador = true;
          this.cargarImagenUsuarioEntrenador(entrenador.entrenadorId);
        } else {
          // Swal.fire({
          //   icon: 'info',
          //   title: 'Información',
          //   text: 'Este usuario no tiene asignado un entrenador.',
          // });
          this.mostrarImagenEntrenador = false;
        }
      },
      error: (error: any) => {
        console.error(error);
        if (error.status === 401) {
          // Manejo de error 401 si es necesario
        } else if (error === "Ocurrió un error en la solicitud. Por favor, inténtelo de nuevo más tarde.") {
          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: 'Este número de telefono no existe.',
          });
          this.limpiarCampos();
        } else {
          // Swal.fire({
          //   icon: 'info',
          //   title: 'Informativo',
          //   text: 'Este usuario no tiene asignado un entrenador.',
          // });
          this.mostrarImagenEntrenador = false;
        }
      },
      complete: () => {
        console.log('Operación completada.');
      },
    });
  }

  obtenerUsuarioPorCorreo(telefono: any) {
    // const cedula = this.formularioUsuario.value.cedula;

    this._usuarioServicio.obtenerUsuarioPorcorreo(telefono).pipe(
      switchMap((usuario: any) => {
        this.limpiarCampos();
        this.clienteSeleccionado = usuario.nombreCompleto;

        console.log(usuario);

        return this._mebresiaServicio.getMembresiaByUsuario2(usuario.idUsuario).pipe(
          catchError(() => {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: 'Este usuario aún no tiene asignada una membresía.',
            });
            this.mostrarImagenEntrenador = false;
            this.imagenSeleccionadaCliente = null;
            this.limpiarCampos();
            return of(null); // evita lanzar error y cortar el flujo feo
          }),
          map((data: any) => ({ usuario, membresia: data }))
        );
      }),
      switchMap(({ usuario, membresia }) => {
        if (!membresia) {
          return EMPTY; // Detiene la ejecución si no hay membresía
        }

        console.log(membresia);
        if (membresia?.status) {
          // ✅ Ahora sí cargamos la imagen porque sabemos que tiene membresía
          this.cargarImagenUsuario(usuario.idUsuario);

          const detalles = membresia.value[0].membresia;
          const fechaVencimiento = new Date(membresia.value[0].fechaVencimiento);
          const hoy = new Date();

          // Calcular diferencia en días
          let diasRestantes = 0; // Valor por defecto
          if (membresia.value[0].estado !== 'Pendiente') {
            const diffTime = fechaVencimiento.getTime() - hoy.getTime();
            diasRestantes = Math.max(Math.ceil(diffTime / (1000 * 60 * 60 * 24)), 0);
          }

          this.diasMembresiaSeleccionado = detalles.duracionDias;
          this.nombreMembresiaSeleccionado = detalles.nombre;
          this.precioMembresiaSeleccionado = this.formatearNumero(detalles.precio);
          this.activoMembresiaSeleccionado = detalles.esActivo;
          this.fechaVencimientoSeleccionado = this.formatearFecha(membresia.value[0].fechaVencimiento);
          this.estadoMembresiaSeleccionado = membresia.value[0].estado;
          this.diasRestantesMembresia = diasRestantes; // 👈 Nuevo campo con días restantes
          this.mostrarImagenEntrenador = true;
        } else {
          Swal.fire({
            icon: 'info',
            title: 'Información',
            text: 'No se encontró una asignación para el usuario especificado.',
          });
          this.mostrarImagenEntrenador = false;
          this.limpiarCampos();
          return EMPTY;
        }

        return this._entrenadorServicio.getAsignacionesPorCliente(usuario.idUsuario);
      })
    ).subscribe({
      next: (asignaciones: any) => {
        if (asignaciones && asignaciones.length > 0) {
          const entrenador = asignaciones[0];
          this.NombreEntrenadorSeleccionado = entrenador.nombreEntrenador;
          this.mostrarImagenEntrenador = true;
          this.cargarImagenUsuarioEntrenador(entrenador.entrenadorId);
        } else {
          // Swal.fire({
          //   icon: 'info',
          //   title: 'Información',
          //   text: 'Este usuario no tiene asignado un entrenador.',
          // });
          this.mostrarImagenEntrenador = false;
        }
      },
      error: (error: any) => {
        console.error(error);
        if (error.status === 401) {
          // Manejo de error 401 si es necesario
        } else if (error === "Ocurrió un error en la solicitud. Por favor, inténtelo de nuevo más tarde.") {
          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: 'Este correo no existe.',
          });
          this.limpiarCampos();
        } else {
          // Swal.fire({
          //   icon: 'info',
          //   title: 'Informativo',
          //   text: 'Este usuario no tiene asignado un entrenador.',
          // });
          this.mostrarImagenEntrenador = false;
        }
      },
      complete: () => {
        console.log('Operación completada.');
      },
    });
  }

  limpiarCampos() {


    this.precioMembresiaSeleccionado = null;
    this.precioCopiaMembresiaSeleccionado = null;
    this.nombreMembresiaSeleccionado = null;
    this.activoMembresiaSeleccionado = null;
    this.estadoMembresiaSeleccionado = null;
    this.fechaVencimientoSeleccionado = null;
    this.diasMembresiaSeleccionado = null;
    this.diasRestantesMembresia = null;
    this.imagenSeleccionada = null;
    this.imagenSeleccionadaCliente = null;
    this.NombreEntrenadorSeleccionado = null;
    this.clienteSeleccionado = null;
  }


  private cargarImagenUsuario(idProducto: number) {
    this._usuarioServicio.obtenerImagenUsuario(idProducto).subscribe(
      (response: any) => {
        if (response && response.imagenUrl) {
          this.imagenSeleccionadaCliente = `${response.imagenUrl}`;
        } else {
          console.error('Imagen no disponible');
          this.imagenSeleccionadaCliente = 'ruta/de/imagen/predeterminada.png'; // O deja nulo
        }
      },
      (error: any) => {
        console.error('Error al cargar la imagen:', error);
        this.imagenSeleccionada = 'ruta/de/imagen/predeterminada.png'; // Imagen predeterminada si falla
      }
    );
  }



  private cargarImagenUsuarioEntrenador(idProducto: number) {
    this._usuarioServicio.obtenerImagenUsuario(idProducto).subscribe(
      (response: any) => {
        if (response && response.imagenUrl) {
          this.imagenSeleccionada = `${response.imagenUrl}`;
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


}
