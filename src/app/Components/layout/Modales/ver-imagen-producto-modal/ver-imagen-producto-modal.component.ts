import { Component, Inject, ViewChild, ElementRef } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-ver-imagen-producto-modal',
  templateUrl: './ver-imagen-producto-modal.component.html',
  styleUrl: './ver-imagen-producto-modal.component.css',

})
export class VerImagenProductoModalComponent {
  imageData: SafeResourceUrl;
  @ViewChild('imageContainer') imageContainer!: ElementRef;

  zoomLevel: number = 1;
  isPanning: boolean = false;
  startX: number = 0;
  startY: number = 0;
  offsetX: number = 0;
  offsetY: number = 0;

  constructor(
    public dialogRef: MatDialogRef<VerImagenProductoModalComponent>,
    private sanitizer: DomSanitizer,
    @Inject(MAT_DIALOG_DATA) public data: { imageData: string }) {
    // this.imageData = this.sanitizer.bypassSecurityTrustUrl(data.imageData);
    this.imageData = this.sanitizer.bypassSecurityTrustResourceUrl(data.imageData);
  }

  cerrarDialog() {
    this.dialogRef.close();
  }

  zoomIn() {
    this.zoomLevel += 0.1;
  }

  zoomOut() {
    if (this.zoomLevel > 0.1) {
      this.zoomLevel -= 0.1;
    }
  }

  resetZoom() {
    this.zoomLevel = 1;
    this.startX = 0;
    this.startY = 0;
    this.offsetX = 0;
    this.offsetY = 0;
  }

  // startPan(event: MouseEvent) {
  //   this.isPanning = true;
  //   this.startX = event.clientX;
  //   this.startY = event.clientY;
  // }
  startPan(event: MouseEvent | TouchEvent) {
    this.isPanning = true;
    if (event instanceof MouseEvent) {
      this.startX = event.clientX;
      this.startY = event.clientY;
    } else if (event instanceof TouchEvent) {
      this.startX = event.touches[0].clientX;
      this.startY = event.touches[0].clientY;
    }
  }

  endPan() {
    this.isPanning = false;
  }

  // panImage(event: MouseEvent) {
  //   if (this.isPanning) {
  //     const deltaX = event.clientX - this.startX;
  //     const deltaY = event.clientY - this.startY;
  //     this.offsetX += deltaX;
  //     this.offsetY += deltaY;
  //     this.startX = event.clientX;
  //     this.startY = event.clientY;
  //   }
  // }
  panImage(event: MouseEvent | TouchEvent) {
    if (this.isPanning) {
      let deltaX, deltaY;
      if (event instanceof MouseEvent) {
        deltaX = event.clientX - this.startX;
        deltaY = event.clientY - this.startY;
        this.startX = event.clientX;
        this.startY = event.clientY;
      } else if (event instanceof TouchEvent) {
        deltaX = event.touches[0].clientX - this.startX;
        deltaY = event.touches[0].clientY - this.startY;
        this.startX = event.touches[0].clientX;
        this.startY = event.touches[0].clientY;
      }
      this.offsetX += deltaX!;
      this.offsetY += deltaY!;
    }
  }
}
