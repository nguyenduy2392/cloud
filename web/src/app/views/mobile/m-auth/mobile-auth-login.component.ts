import { NgClass } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import {
  FormsModule,
  ReactiveFormsModule,
  UntypedFormBuilder,
  Validators,
  type UntypedFormGroup,
} from '@angular/forms'
import { Router, RouterLink } from '@angular/router'
import { AuthService } from '@/app/services/auth.service'

@Component({
  selector: 'app-mobile-auth-login',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, RouterLink, NgClass],
  templateUrl: './mobile-auth-login.component.html',
  styleUrl: './mobile-auth-login.component.scss',
})
export class MobileAuthLogin implements OnInit {
  signInForm!: UntypedFormGroup
  submitted = false
  errorMessage = ''
  showPassword = false

  constructor(
    public fb: UntypedFormBuilder,
    private router: Router,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.signInForm = this.fb.group({
      identity: ['', [Validators.required]],
      userName: ['', [Validators.required]],
      password: ['', [Validators.required]],
      rememberMe: [false],
    })
  }

  get formValues() {
    return this.signInForm.controls
  }

  login() {
    this.submitted = true
    if (this.signInForm.valid) {
      const identity = this.formValues['identity'].value as string
      const userName = this.formValues['userName'].value as string
      const password = this.formValues['password'].value as string

      this.authService.login({ identity, userName, password }).subscribe({
        next: () => {
          // Luôn về trang chủ mobile (m-dashboard tại route `index`)
          void this.router.navigateByUrl('/')
        },
        error: (err: unknown) => {
          const e = err as {
            error?: { message?: string }
            message?: string
            statusText?: string
            toString?: () => string
          }
          const msg =
            e?.error?.message ??
            e?.message ??
            e?.statusText ??
            e?.toString?.() ??
            'Đăng nhập thất bại'
          this.errorMessage = msg
          setTimeout(() => {
            this.errorMessage = ''
          }, 4000)
        },
      })
    }
  }

  onGoogleContinue(): void {
    // Chưa tích hợp OAuth — giữ nút theo mockup UI
  }
}
