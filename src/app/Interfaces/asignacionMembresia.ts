export interface AsignacionMembresia {


  asignacionId?: number;
  idUsuario: number;
  nombreUsuario?: string;
  telefonoUsuario?: string;
  idMembresia: number;
  nombreMembresia?: string;
  fechaVencimiento?: string;
  estado?: string;
  imagenUrl?: string | null;
}
