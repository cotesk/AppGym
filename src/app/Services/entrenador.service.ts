import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AsignacionEntrenadorCliente } from '../Interfaces/asignacionEntrenadorCliente';
import { EntrenadorCliente } from '../Interfaces/entrenadorCliente';
import { environment } from '../../environments/environment';
import { ReponseApi } from '../Interfaces/reponse-api';

@Injectable({
  providedIn: 'root'
})
export class EntrenadorClienteService {

  private urlApi: string = environment.endpoint + "EntrenadorCliente/";

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

  constructor(private http: HttpClient) { }

  // Asignar entrenador a cliente
  asignarEntrenadorACliente(asignacion: AsignacionEntrenadorCliente): Observable<any> {
    return this.http.post(`${this.urlApi}asignar-entrenador`, asignacion, { headers: this.getHeaders() });
  }

  // Obtener asignación por ID
  getAsignacion(id: number): Observable<ReponseApi> {
    return this.http.get<any>(`${this.urlApi}${id}`, { headers: this.getHeaders() });
  }

  // Obtener todas las asignaciones
  getTodasLasAsignaciones(): Observable<any> {
    return this.http.get<any>(`${this.urlApi}traerTodo`, { headers: this.getHeaders() });
  }

  // Obtener asignaciones por cliente
  getAsignacionesPorCliente(clienteId: number): Observable<ReponseApi[]> {
    return this.http.get<any>(`${this.urlApi}cliente/${clienteId}`);
  }

  // Obtener asignaciones por entrenador
  getAsignacionesPorEntrenador(entrenadorId: number): Observable<ReponseApi> {
    return this.http.get<any>(`${this.urlApi}entrenador/${entrenadorId}`, { headers: this.getHeaders() });
  }

   // Editar una asignación existente
   editarAsignacion(id: number, asignacionEditada: any): Observable<any> {
    return this.http.put(`${this.urlApi}editar-asignacion/${id}`, asignacionEditada,  { headers: this.getHeaders() });
  }

  // Eliminar una asignación específica
  eliminarAsignacion(id: number): Observable<any> {
    return this.http.delete(`${this.urlApi}eliminar-asignacion?asignacionId=${id}`, { headers: this.getHeaders() });
  }


}



