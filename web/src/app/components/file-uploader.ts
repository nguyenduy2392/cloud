import { Component, Input, type OnInit } from '@angular/core'
import {
  DROPZONE_CONFIG,
  DropzoneConfigInterface,
  DropzoneModule,
} from 'ngx-dropzone-wrapper'

type UploadedFile = {
  name: string
  size: number
  type: string
  dataURL?: string
}

const DEFAULT_DROPZONE_CONFIG: DropzoneConfigInterface = {
  // Change this to your upload POST address:
  url: 'https://httpbin.org/post',
  maxFilesize: 50,
  acceptedFiles: 'image/*',
}

@Component({
  selector: 'FileUploader',
  standalone: true,
  imports: [DropzoneModule],
  template: ` <dropzone
      class="dropzone"
      [config]="dropzoneConfig"
      [message]="dropzone"
      (success)="onUploadSuccess($event)"
    ></dropzone>

    @if (showPreview && uploadedFiles) {
      <div class="dropzone-previews mt-3" id="file-previews">
        @for (file of uploadedFiles; track $index) {
          <div
            class="card mt-1 mb-0 shadow-none border dz-processing dz-success dz-complete dz-image-preview"
          >
            <div class="p-2">
              <div class="row align-items-center">
                <div class="col-auto">
                  <img
                    data-dz-thumbnail=""
                    [src]="file.dataURL"
                    class="avatar-sm rounded bg-light"
                  />
                </div>
                <div class="col ps-0">
                  <a href="javascript:void(0);" class="text-muted fw-bold">{{
                    file.name
                  }}</a>
                  <p class="mb-0" data-dz-size="">
                    <strong>{{ file.size }}</strong> KB
                  </p>
                </div>
                <div class="col-auto">
                  <a
                    href="javascript:void(0);"
                    class="btn btn-link btn-lg text-dark"
                    (click)="removeFile($index)"
                  >
                    <i class="ti ti-x"></i>
                  </a>
                </div>
              </div>
            </div>
          </div>
        }
      </div>
    }`,
  providers: [
    {
      provide: DROPZONE_CONFIG,
      useValue: DEFAULT_DROPZONE_CONFIG,
    },
  ],
})
export class FileUploader implements OnInit {
  @Input() showPreview: boolean = false
  uploadedFiles: UploadedFile[] = []

  dropzoneConfig: DropzoneConfigInterface = {
    url: 'https://httpbin.org/post',
    maxFilesize: 50,
    clickable: true,
    addRemoveLinks: true,
  }

  dropzone = `<div class="dz-message needsclick">
    <i class="ti ti-cloud-upload h1 text-muted"></i>
     <h3>Drop files here or click to upload.</h3>
    <span class="text-muted fs-13">(This is just a demo dropzone. Selected files are <strong>not</strong>
                actually uploaded.)</span>
  </div>`

  ngOnInit(): void {
    if (this.showPreview == true) {
      this.dropzoneConfig.previewsContainer = false
    }
  }
  // File Upload
  imageURL: string = ''
  onUploadSuccess(event: UploadedFile[]) {
    setTimeout(() => {
      this.uploadedFiles.push(event[0])
    }, 0)
  }

  // File Remove
  removeFile(index: number) {
    this.uploadedFiles.splice(index, 1)
  }
}
