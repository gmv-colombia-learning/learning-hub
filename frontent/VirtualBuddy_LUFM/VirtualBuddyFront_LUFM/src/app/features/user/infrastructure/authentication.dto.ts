export interface LoginRequestDto {
  readonly email: string;
  readonly password: string;
}

export interface AuthResponseDto {
  readonly token: string;
  readonly email: string;
  readonly fullName: string;
}
