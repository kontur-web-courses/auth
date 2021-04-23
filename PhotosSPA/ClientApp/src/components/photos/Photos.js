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
    this.state = { photos: [], loading: true };
  }

  componentDidMount() {
    this.populatePhotos();
  }

  // NOTE: Логика отрисовки страницы такая:
  // в зависимости от состояния либо показывается надпись «Загрузка...»,
  // либо показывается информация о фотографиях.
  render() {
    const contents = this.state.loading ? (
      <p>
        <em>Загрузка...</em>
      </p>
    ) : (
      this.renderPhotos(this.state.photos)
    );

    return (
      <div className="photosContainer">
        <h1>Все фотографии</h1>
        {contents}
      </div>
    );
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

    // NOTE: Если нет access token, то загрузка прекращается, а фотографии не показываются.
    if (!accessToken) {
      this.setState({ photos: [], loading: false });
      return;
    }

    // NOTE: Если есть access token, то можно попробовать получить информацию о фотографиях
    const response = await fetch(
      `${PhotosServiceUrl}/api/photos?ownerId=${encodeURIComponent(userId)}`,
      {
        headers: { Authorization: `Bearer ${accessToken}` },
      }
    );

    // NOTE: Похоже токен больше не действует, поэтому надо попробовать его обновить.
    // Компонент Photos вложен в компонент AuthorizeRoute, поэтому после успешного выполнения signIn
    // произойдет обновление страницы, а значит попытка загрузить фотографии компонентом Photos повторится.
    if (response.status === 401) {
      await authService.signIn();
      return;
    }

    // NOTE: Получение результата в виде JSON
    const data = await response.json();
    // NOTE: Обновление состояния страницы: фотографии получены, загрузка закончена.
    this.setState({ photos: data, loading: false });
  }
}
