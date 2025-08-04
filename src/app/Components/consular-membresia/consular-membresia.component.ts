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
import { of } from 'rxjs';


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


      cedula: ['', [Validators.required, Validators.pattern('[0-9]*'), Validators.maxLength(10)]]
    });




   }

   ngOnInit(): void {


  }


  verImagen(usuario: any): void {
    this.dialog.open(VerImagenProductoModalComponent, {
      data: {
        imageData: usuario
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


  BuscarUsuario(){
    this.obtenerUsuarioPorCedula();
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
  obtenerUsuarioPorCedula() {
    const cedula = this.formularioUsuario.value.cedula;

    this._usuarioServicio.obtenerUsuarioPorCedula(cedula).pipe(
      switchMap((usuario: any) => {
        this.limpiarCampos();
        this.clienteSeleccionado = usuario.nombreCompleto;
        this.cargarImagenUsuario(usuario.idUsuario);

        return this._mebresiaServicio.getMembresiaByUsuario2(usuario.idUsuario).pipe(
          catchError(() => {
            Swal.fire({
              icon: 'error',
              title: 'ERROR',
              text: 'No se encontró una membresía para el usuario.',
            });
            this.mostrarImagenEntrenador = false; // Oculta la imagen del entrenador
            this.limpiarCampos();
            throw new Error('No se encontró una membresía.');
          }),
          map((data: any) => ({ usuario, membresia: data }))
        );
      }),
      switchMap(({ usuario, membresia }) => {
        if (membresia?.status) {
          const detalles = membresia.value[0].membresia;
          this.diasMembresiaSeleccionado = detalles.duracionDias;
          this.nombreMembresiaSeleccionado = detalles.nombre;
          this.precioMembresiaSeleccionado = this.formatearNumero(detalles.precio);
          this.activoMembresiaSeleccionado = detalles.esActivo;
          this.fechaVencimientoSeleccionado =  this.formatearFecha(membresia.value[0].fechaVencimiento);
          this.estadoMembresiaSeleccionado = membresia.value[0].estado;
          this.mostrarImagenEntrenador = true; // Muestra la imagen del entrenador
        } else {
          Swal.fire({
            icon: 'info',
            title: 'Información',
            text: 'No se encontró una asignación para el usuario especificado.',
          });
          this.mostrarImagenEntrenador = false; // Oculta la imagen del entrenador
          this.limpiarCampos();
        }

        return this._entrenadorServicio.getAsignacionesPorCliente(usuario.idUsuario);
      })
    ).subscribe({
      next: (asignaciones: any) => {
        if (asignaciones) {
          const entrenador = asignaciones[0];
          this.NombreEntrenadorSeleccionado = entrenador.nombreEntrenador;
          this.mostrarImagenEntrenador = true; // Muestra la imagen del entrenador
          this.cargarImagenUsuarioEntrenador(entrenador.entrenadorId);
        } else {
          Swal.fire({
            icon: 'info',
            title: 'Información',
            text: 'No se encontró una asignación de entrenador para el usuario.',
          });
          this.mostrarImagenEntrenador = false; // Oculta la imagen del entrenador
        }
      },
      error: (error: any) => {
        console.error(error);
        if(error.status == 401){



        }else if(error =="Ocurrió un error en la solicitud. Por favor, inténtelo de nuevo más tarde."){
          Swal.fire({
            icon: 'error',
            title: 'ERROR',
            text: 'Este numero de cedula no existe.',
          });
          this.limpiarCampos();
        }
        else{

          Swal.fire({
            icon: 'info',
            title: 'Informativo',
            text: 'Este Usuario no se le a asignado un entrenador.',
          });
          this.mostrarImagenEntrenador = false; // Oculta la imagen del entrenador
          // this.limpiarCampos();

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
    this.imagenSeleccionada =null;
    this.imagenSeleccionadaCliente = null;
    this.NombreEntrenadorSeleccionado = null;
    this.clienteSeleccionado=null;
  }


  private cargarImagenUsuario(idProducto: number) {
    this._usuarioServicio.obtenerImagenUsuario(idProducto).subscribe(
      (response: any) => {
        if (response && response.imageData) {
          this.imagenSeleccionadaCliente = `data:image/png;base64,${response.imageData}`;
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


}
