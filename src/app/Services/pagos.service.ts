import { environment } from './../../environments/environment.development';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';

import { ReponseApi } from '../Interfaces/reponse-api';
import { Pagos } from '../Interfaces/pagos';

@Injectable({
  providedIn: 'root'
})
export class PagosService {

  private urlApi: string = environment.endpoint + "Pagos/";

  constructor(private http: HttpClient) { }

  // Obtener el token y configurar los headers con autorización
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem("authToken");
    if (!token) {
      console.error('No se encontró un token JWT en el almacenamiento local.');
      throw new Error('No se encontró un token JWT en el almacenamiento local.');
    }
    return new HttpHeaders({
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`
    });
  }

  // Registrar un nuevo pago
  registrarPago(pago: Pagos): Observable<any> {
    return this.http.post(`${this.urlApi}Registrar`, pago, { headers: this.getHeaders() })
      .pipe(
        catchError(this.handleError)
      );
  }


 // Registrar un nuevo pago pendiente
 registrarPagoCalendario(pago: Pagos): Observable<any> {
  return this.http.post(`${this.urlApi}RegistrarCalendario`, pago, { headers: this.getHeaders() })
    .pipe(
      catchError(this.handleError)
    );
}

  // Obtener los pagos de un usuario
  obtenerPagos(idUsuario: number): Observable<any> {
    return this.http.get(`${this.urlApi}ObtenerPagos/${idUsuario}`, { headers: this.getHeaders() })
      .pipe(
        catchError(this.handleError)
      );
  }

  obtenerEstadoCalendario(idUsuario: number): Observable<any> {
    return this.http.get(`${this.urlApi}BuscarEstadoCalendario/${idUsuario}`, { headers: this.getHeaders() })
      .pipe(
        catchError(this.handleError)
      );
  }

  // Obtener todos los pagos
  obtenerTodosLosPagos(): Observable<any> {
    return this.http.get(`${this.urlApi}ObtenerTodosLosPagos`, { headers: this.getHeaders() })
      .pipe(
        catchError(this.handleError)
      );
  }

  listaPaginada(page: number = 1, pageSize: number = 5, searchTerm: string = ''): Observable<any> {
    const url = `${this.urlApi}ListaPaginada?page=${page}&pageSize=${pageSize}&searchTerm=${searchTerm}`;
    return this.http.get<any>(url, { headers: this.getHeaders() }).pipe(
      catchError(this.handleError)
    );
  }

  // Manejo de errores
  private handleError(error: any): Observable<never> {
    console.error('Ocurrió un error:', error);

    // Crear un objeto de error personalizado que contenga tanto el status como el mensaje
    const errorResponse = {
      error:  error?.msg || 'Error desconocido',
      status: error.status || 500,  // Si no existe un status, usamos 500 (error interno del servidor)
      message: error.message || 'Error al realizar la solicitud.',
      msg: error.error?.msg || 'Error desconocido', // Si el servidor envió un mensaje específico
    };

    // Regresar el error en el observable para que pueda ser manejado en la suscripción
    return throwError(() => errorResponse);
  }
}
