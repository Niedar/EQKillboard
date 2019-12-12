import React, { Component } from 'react';
import { Layout } from 'antd';
import { Route, Switch, Redirect } from 'react-router-dom'
import HomePage from './HomePage';
import CharacterPage from './CharacterPage';
import GuildPage from './GuildPage';
import ZonePage from './ZonePage';
import './App.css';
import SiteHeader from './SiteHeader';
import { SeasonContext } from './SeasonContext'

const { Footer, Content } = Layout;

const SeasonRoutes = ({ match }) => {
  return (
    <SeasonContext.Provider value={parseInt(match.params.season)}>
      <SiteHeader />
      <Layout>
        <Content>
          <Switch>
            <Route exact path={`${match.path}/`} component={HomePage}/>
            <Route path={`${match.path}/:cursorDirection(before|after)/:cursor`} component={HomePage}/>
            <Route exact path={`${match.path}/character/:characterId`} component={CharacterPage}/>
            <Route exact path={`${match.path}/character/:characterId/:cursorDirection(before|after)/:cursor`} component={CharacterPage}/>
            <Route exact path={`${match.path}/guild/:guildId`} component={GuildPage}/>
            <Route exact path={`${match.path}/guild/:guildId/:cursorDirection(before|after)/:cursor`} component={GuildPage}/>
            <Route exact path={`${match.path}/zone/:zoneId`} component={ZonePage}/>
            <Route exact path={`${match.path}/zone/:zoneId/:cursorDirection(before|after)/:cursor`} component={ZonePage} />
            <Route>
              <Redirect to={`${match.path}/`} />
            </Route>
          </Switch>
        </Content>
      </Layout>
    </SeasonContext.Provider>
  )
}

class App extends Component {
  render() {
    return (
      <div className="container">
        <Layout style={{ minHeight: '100vh' }}>
          <Switch>
            <Route path="/:season(1|2)" component={SeasonRoutes} />
            <Route>
              <Redirect to="/2/" />
            </Route>
          </Switch>
          <Footer></Footer>
        </Layout>
      </div>
    );
  }
}

export default App;
