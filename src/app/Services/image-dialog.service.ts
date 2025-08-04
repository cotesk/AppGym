import { ImagenDialogComponent } from './../Components/layout/Modales/imagen-dialog/imagen-dialog.component';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';


@Injectable({
  providedIn: 'root',
})
export class ImageDialogService {
  constructor(private dialog: MatDialog) {}

  openImageDialog(imageData: string): void {
    this.dialog.open(ImagenDialogComponent, {
      data: imageData,
      maxWidth: '90vw',
      maxHeight: '90vh',
    });
  }
}
