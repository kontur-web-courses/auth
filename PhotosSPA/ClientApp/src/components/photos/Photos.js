import React, { Component } from "react";
import authService from "../api-authorization/AuthorizeService";
import { PhotosServiceUrl } from "./PhotosConstants";
import "./Photos.css";

export class Photos extends Component {
  static displayName = Photos.name;

  constructor(props) {
    super(props);
    // NOTE: Сразу после перехода пользователя на страницу фотографий состояние такое:
    // фотографий еще нет, но загрузка уже ведется.
    this.state = { photos: [], loading: true, errorMessage: null };
  }

  componentDidMount() {
    this.populatePhotos();
  }

  // NOTE: Здесь формируется разметка страницы
  render() {
    return (
      <div className="photosContainer">
        <h1>Все фотографии</h1>
        {this.renderContents()}
      </div>
    );
  }

  // NOTE: Логика отрисовки содержимого страницы такая:
  // в зависимости от состояния либо показывается надпись «Загрузка...»,
  // либо выводится сообщение об ошибки,
  // либо показывается информация о фотографиях.
  renderContents() {
    if (this.state.loading) {
      return (
        <p>
          <em>Загрузка...</em>
        </p>
      );
    }

    if (this.state.errorMessage != null) {
      return (
        <p>
          <em>{this.state.errorMessage}</em>
        </p>
      );
    }

    return this.renderPhotos(this.state.photos);
  }

  renderPhotos(photos) {
    return <div>{photos.map((photo) => this.renderPhoto(photo))}</div>;
  }

  // NOTE: Логика отрисовки информации про отдельную фотографию.
  // Изначально показывается ID и имя файла в сервисе фотографий.
  renderPhoto(photo) {
    return (
      <div key={photo.id} className="photoContainer">
        <h3>{photo.title}</h3>
        <ul>
          <li>
            <b>ID:</b> <span>{photo.id}</span>
          </li>
          <li>
            <b>Файл:</b> <span>{photo.fileName}</span>
          </li>
          <img src={photo.url} className="photo" />
        </ul>
      </div>
    );
  }

  // NOTE: Запрос, получающий фотографии пользователя
  async populatePhotos() {
    // NOTE: Получение информации о пользователе и его access token.
    const user = await authService.getUser();
    const userId = user && user.sub;
    const accessToken = await authService.getAccessToken();

    // NOTE: Если нет access token, то загрузка прекращается и показывается сообщение об ошибке.
    if (!accessToken) {
      this.setState({
        photos: [],
        loading: false,
        errorMessage: "Отказано в доступе",
      });
      return;
    }

    // NOTE: Если есть access token, то можно попробовать получить информацию о фотографиях
    const response = await fetch(
      `${PhotosServiceUrl}/api/photos?ownerId=${encodeURIComponent(userId)}`,
      {
        headers: { Authorization: `Bearer ${accessToken}` },
      }
    );

    // NOTE: Похоже токен больше не действует,
    // поэтому завершаем загрузку и выводим сообщение об ошибке
    if (response.status === 401) {
      this.setState({
        photos: [],
        loading: false,
        errorMessage: "Отказано в доступе",
      });
      return;
    }

    // NOTE: Получение результата в виде JSON
    const data = await response.json();
    // NOTE: Обновление состояния страницы: фотографии получены, загрузка закончена.
    this.setState({ photos: data, loading: false, errorMessage: null });
  }
}
