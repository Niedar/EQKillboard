import React, { Component } from 'react';
import { Link } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import ZoneInfoQuery from './ZoneInfoQuery';
import Killmails from './Killmails';
import { Button, Icon } from 'antd';
import { Spin, Row, Col } from 'antd';
import { SeasonContext } from './SeasonContext';



// TODO: turn this into a component
const getPaginationButtons = (pageInfo, baseUrl) => {
  let buttonStyle = {
    marginTop: "20px",
    marginRight: "20px"
  }
  var nextButton;
  var prevButton;

  nextButton = (
    <Button type="primary" style={buttonStyle} disabled={!pageInfo.hasNextPage}>
      <Link to={`${baseUrl}/after/${pageInfo.endCursor}`} style={{ textDecoration: "none"}}>
        Forward<Icon type="right" />
      </Link>
    </Button>
  )
  prevButton = (
    <Button type="primary" style={buttonStyle} disabled={!pageInfo.hasPreviousPage}>
      <Link to={`${baseUrl}/before/${pageInfo.startCursor}`} style={{ textDecoration: "none"}}>
        <Icon type="left" />Backward
      </Link>
    </Button>
  )

  return (
    <React.Fragment>
      <Row type={"flex"} justify={"end"}>
        <Col>
          {prevButton}
          {nextButton}
        </Col>
      </Row>
    </React.Fragment>
  );
};

class ZonePage extends Component {
  static contextType = SeasonContext;
  render() {
    const season = this.context;
    return (
      <React.Fragment>
        <ZoneInfoQuery zoneId={parseInt(this.props.match.params.zoneId)}>
        {({ loading, error, data}) => {
            if (loading) return null;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div style={{marginLeft: "10px"}}>
                <h1>{data.zoneById.name}</h1>
                <h2>Kills: {data.zoneById.killmailsByZoneId.totalCount}</h2>
              </div>
            );
        }}
        </ZoneInfoQuery>
        <KillmailsQuery
          before={this.props.match.params.cursorDirection === "before" ? this.props.match.params.cursor : null}
          after={this.props.match.params.cursorDirection === "after" ? this.props.match.params.cursor : null}
          zoneId={parseInt(this.props.match.params.zoneId)}
        >
        {({ loading, error, data}) => {
            if (loading) return <Spin size="large" />;
            if (error) return `Error! ${error.message}`;
            
            return (
              <div>
                { getPaginationButtons(data.allKillmails.pageInfo, `/${season}/zone/${this.props.match.params.zoneId}`) }
                <Killmails killmails={data.allKillmails.nodes} zoneId={this.props.match.params.zoneId}/>
                { getPaginationButtons(data.allKillmails.pageInfo, `/${season}/zone/${this.props.match.params.zoneId}`) }
              </div>
            );
        }}
        </KillmailsQuery>
      </React.Fragment>
    );
  }
}

export default ZonePage;
