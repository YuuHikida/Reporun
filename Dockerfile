# �J�X�^�}�C�Y���@��Visual Studio�̗��p���@�Ɋւ�����ւ̃����N
#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ASP.NET Core 6.0�����^�C�����܂ފ�{�C���[�W���擾
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# �R���e�i���̍�ƃf�B���N�g���� /app �ɐݒ�
WORKDIR /app

# �R���e�i���|�[�g80�iHTTP�j��443�iHTTPS�j�ŒʐM���邱�Ƃ�����
EXPOSE 80
EXPOSE 443

# .NET Core 6.0 SDK���܂ރr���h�p�̊�{�C���[�W���擾
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# �r���h�X�e�[�W�ł̍�ƃf�B���N�g���� /src �ɐݒ�
WORKDIR /src

# �v���W�F�N�g�t�@�C���icsproj�j���R���e�i�ɃR�s�[
COPY ["ReportSystem.csproj", "."]

# �v���W�F�N�g�̈ˑ��֌W�𕜌�
RUN dotnet restore "./ReportSystem.csproj"

# �c��̃v���W�F�N�g�t�@�C�����R���e�i�ɃR�s�[
COPY . .

# �r���h�X�e�[�W�̍�ƃf�B���N�g�����X�V
WORKDIR "/src/."

# �v���W�F�N�g�������[�X�\���Ńr���h���A/app/build �ɏo��
RUN dotnet build "ReportSystem.csproj" -c Release -o /app/build

# �r���h�X�e�[�W����publish�X�e�[�W�ֈڍs
FROM build AS publish

# �A�v���P�[�V�������p�u���b�V���i�R���p�C���ƃ��\�[�X�̃p�b�P�[�W���O�j
RUN dotnet publish "ReportSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# base�X�e�[�W����ŏI�I�Ȏ��s�C���[�W���쐬
FROM base AS final

# �ŏI�C���[�W�ł̍�ƃf�B���N�g���� /app �ɐݒ�
WORKDIR /app

# publish�X�e�[�W�ō쐬���ꂽ�t�@�C�����ŏI�C���[�W�ɃR�s�[
COPY --from=publish /app/publish .

# �R���e�i���N�������ۂɎ��s�����R�}���h��ݒ�
ENTRYPOINT ["dotnet", "ReportSystem.dll"]
