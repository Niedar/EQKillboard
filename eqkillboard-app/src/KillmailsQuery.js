import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"
import { SeasonContext } from "./SeasonContext"

export default class KillmailsQuery extends Component {
  static contextType = SeasonContext;

  render() {
    const { characterId, guildId, zoneId, children, after, before } = this.props;
    const season = this.context;
    var query;
    var id, first, last;

    if (before) {
      last = 50;
    } else {
      first = 50
    }

    if (characterId) {
      query = GET_CHARACTERKILLMAILS;
      id = characterId;
    } else if (guildId) {
      query = GET_GUILDKILLMAILS;
      id = guildId
    } else if (zoneId) {
      query = GET_ZONEKILLMAILS;
      id = zoneId
    } else {
      query = GET_ALLKILLMAILS;
    }

    return (
      <Query query={query} variables={{ season, id, first, last, after, before }}>
        {result => children(result)}
      </Query>
    )
  }
}

export const KillmailDataFragment = gql`
fragment KillmailData on Killmail {
  nodeId
  id,
  characterByVictimId {
    id,
    name
  },
  guildByVictimGuildId {
    id, 
    name
  },
  victimLevel,
  characterByAttackerId {
    id,
    name
  },
  guildByAttackerGuildId {
    id,
    name
  },
  attackerLevel,
  zoneByZoneId {
    id,
    name
  },
  killedAt  
}
`;

const GET_ALLKILLMAILS = gql`
  query allKilmails($season: Int, $first: Int, $last: Int, $after: Cursor, $before: Cursor){
    allKillmails(condition: {season: $season}, orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;

const GET_CHARACTERKILLMAILS = gql`
  query characterKilmails($season: Int, $id: Int, $first: Int, $last: Int, $after: Cursor, $before: Cursor) {
    allKillmails(condition: {season: $season}, filter: { or: [{ victimId: { equalTo: $id } }, { attackerId: { equalTo: $id} }] }, orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;

const GET_GUILDKILLMAILS = gql`
  query guildKilmails($season: Int, $id: Int, $first: Int, $last: Int, $after: Cursor, $before: Cursor) {
    allKillmails(condition: {season: $season}, filter: { or: [{ victimGuildId: { equalTo: $id } }, { attackerGuildId: { equalTo: $id} }] }, orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;

const GET_ZONEKILLMAILS = gql`
  query zoneKilmails($season: Int, $id: Int, $first: Int, $last: Int, $after: Cursor, $before: Cursor) {
    allKillmails(condition: {season: $season}, filter: { zoneId: { equalTo: $id } }, orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;