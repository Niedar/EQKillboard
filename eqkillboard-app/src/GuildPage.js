import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import Killmails from './Killmails';
import { Button, Radio, Icon } from 'antd';
import { Spin } from 'antd';



// TODO: turn this into a component
const getPaginationButtons = (pageInfo, baseUrl) => {
  var nextButton;
  var prevButton;

  if (pageInfo.hasNextPage) {
    nextButton = (
      <Button type="primary">
        <Link to={`${baseUrl}/after/${pageInfo.endCursor}`}>
          Forward<Icon type="right" />
        </Link>
      </Button>
    )
  } else {

  }

  if (pageInfo.hasPreviousPage) {
    prevButton = (
      <Button type="primary">
        <Link to={`${baseUrl}/before/${pageInfo.startCursor}`}>
          <Icon type="left" />Backward
        </Link>
      </Button>
    )
  } else {

  }

  return (
    <React.Fragment>
      {prevButton}
      {nextButton}
    </React.Fragment>
  );
};

class GuildPage extends Component {
  render() {
    return (
      <KillmailsQuery
        before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
        after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
        guildId={this.props.match.params.guildId}
      >
      {({ loading, error, data}) => {
          if (loading) return <Spin size="large" />;
          if (error) return `Error! ${error.message}`;
          
          return (
            <div>
              <h1>Guild {this.props.match.params.guildId}</h1>
              { getPaginationButtons(data.allKillmails.pageInfo, `/guild/${this.props.match.params.guildId}`) }
              <Killmails killmails={data.allKillmails.nodes} />
              { getPaginationButtons(data.allKillmails.pageInfo, `/guild/${this.props.match.params.guildId}`) }
            </div>
          );
      }}
      </KillmailsQuery>
    );
  }
}

export default GuildPage;
