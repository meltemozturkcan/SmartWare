import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService, LoginDto } from '../../core/services/auth.services';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class LoginComponent {
  loginDto: LoginDto = {
    usernameOrEmail: '',
    password: ''
  };
  
  // Yeni özellikler
  loginSuccess: boolean = false;
  loginSuccessMessage: string = '';
  errorMessage: string = '';

  constructor(private authService: AuthService) {}

  onSubmit() {
    this.authService.login(this.loginDto).subscribe({
      next: (response) => {
        // Token kaydetme
        this.authService.setToken(response.token);
        
        // Başarılı giriş mesajı
        this.loginSuccess = true;
        this.loginSuccessMessage = `Hoş geldiniz, ${response.username}! Giriş başarılı.`;
        
        // Hata mesajını temizle
        this.errorMessage = '';

        // Formu sıfırlama
        this.loginDto.usernameOrEmail = '';
        this.loginDto.password = '';
      },
      error: (err) => {
        // Hata durumunda
        this.loginSuccess = false;
        this.loginSuccessMessage = '';
        this.errorMessage = 'Giriş başarısız. Kullanıcı adı veya şifre hatalı.';
        console.error('Giriş hatası', err);
      }
    });
  }
}