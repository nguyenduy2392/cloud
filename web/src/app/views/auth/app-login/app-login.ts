import { Component, OnInit } from '@angular/core'
import {
  FormsModule,
  ReactiveFormsModule,
  UntypedFormBuilder,
  Validators,
  type UntypedFormGroup,
} from '@angular/forms'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { currentYear } from '@common/constants'
import { NgClass, NgIf } from '@angular/common'
import { AuthService } from '@/app/services/auth.service'

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, RouterLink, NgClass, NgIf],
  templateUrl: './app-login.html',
  styles: ``,
})
export class AppLogin implements OnInit {
  currentYear = currentYear
  signInForm!: UntypedFormGroup
  submitted: boolean = false

  errorMessage: string = ''

  constructor(
    public fb: UntypedFormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.signInForm = this.fb.group({
      identity: ['', [Validators.required]],
      userName: ['admin', [Validators.required]],
      password: ['', [Validators.required]],
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
          const returnUrl =
            this.route.snapshot.queryParams['returnUrl'] || '/'
          this.router.navigateByUrl(returnUrl)
        },
        error: (err) => {
          const msg =
            err?.error?.message ??
            err?.message ??
            err?.statusText ??
            err?.toString?.() ??
            'Login failed'
          this.errorMessage = msg
          setTimeout(() => {
            this.errorMessage = ''
          }, 3000)
        },
      })
    }
  }
}
